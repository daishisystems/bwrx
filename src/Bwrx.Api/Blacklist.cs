using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bwrx.Api
{
    public class Blacklist
    {
        private static readonly Lazy<Blacklist> Lazy = new Lazy<Blacklist>(() => new Blacklist());
        private string _blacklistUri;
        private string _blacklistCountUri;

        private HttpClient _httpClient;
        private int _maxNumIpAddressesPerHttpRequest;

        public static Blacklist Instance => Lazy.Value;

        public HashSet<string> IpAddresses { get; private set; } = new HashSet<string>();

        public event EventHandlers.IpAddressAddedHandler IpAddressAdded;

        public event EventHandlers.AddIpAddressFailedEventHandler AddIpAddressFailed;

        public event EventHandlers.ListUpdatedHandler BlacklistUpdated;

        public event EventHandlers.GotLatestListEventHandler GotLatestBlacklist;

        public event EventHandlers.GetLatestListFailedEventHandler GetLatestBlacklistFailed;

        public event EventHandlers.CouldNotParseIpAddressEventHandler CouldNotParseIpAddress;

        public void Init(ClientConfigSettings clientConfigSettings)
        {
            if (clientConfigSettings == null) throw new ArgumentNullException(nameof(clientConfigSettings));

            if (!string.IsNullOrEmpty(clientConfigSettings.HttpProxy))
            {
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = new WebProxy(clientConfigSettings.HttpProxy),
                    UseProxy = true
                };
                _httpClient = new HttpClient(httpClientHandler)
                {
                    BaseAddress = new Uri(clientConfigSettings.BlacklistUri)
                };
            }
            else
            {
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(clientConfigSettings.BlacklistUri)
                };
            }
            _blacklistUri = clientConfigSettings.BlacklistUri;
            _blacklistCountUri = clientConfigSettings.BlacklistCountUri;
            _maxNumIpAddressesPerHttpRequest = clientConfigSettings.MaxNumIpAddressesPerHttpRequest;
        }

        public bool IsIpAddressBlacklisted(string ipAddress)
        {
            return IpAddresses.Contains(ipAddress);
        }

        public void UpDate(IEnumerable<string> blacklistedIPAddresses)
        {
            IpAddresses = new HashSet<string>(blacklistedIPAddresses);
            OnBlacklistUpdated(new EventArgs());
        }

        public async Task<HashSet<string>> GetLatestAsync(
            HashSet<string> whitelistedIpAddresses)
        {
            if (whitelistedIpAddresses == null) throw new ArgumentNullException(nameof(whitelistedIpAddresses));

            try
            {
                var bulkDataDownloader = new BulkDataDownloader();
                var recordCount =
                    await bulkDataDownloader.GetRecordCountAsync(_httpClient,
                        _blacklistCountUri + "?tablename=blacklist");

                var numHttpRequestsRequired =
                    bulkDataDownloader.CalcNumHttpRequestsRequired(recordCount.Total, _maxNumIpAddressesPerHttpRequest);
                var paginationSequence = bulkDataDownloader.CalcPaginationSequence(
                    numHttpRequestsRequired,
                    _maxNumIpAddressesPerHttpRequest);
                var data = await bulkDataDownloader.LoadDataAsync<IpAddressMeta>(_httpClient, _blacklistUri,
                    paginationSequence);

                var blacklist = new HashSet<string>();
                foreach (var ipAddressMeta in data)
                    if (!whitelistedIpAddresses.Contains(ipAddressMeta.IpAddress))
                        blacklist.Add(ipAddressMeta.IpAddress);

                OnGotLatestBlacklist(new GotLatestListEventArgs(blacklist.Count));
                return blacklist;
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not get the latest blacklist.";
                OnGetBlacklistFailed(new GetLatestListFailedEventArgs(new Exception(errorMessage, exception)));
                return new HashSet<string>();
            }
        }

        private void OnIpAddressAdded(IpAddressAddedEventArgs e)
        {
            IpAddressAdded?.Invoke(this, e);
        }

        private void OnAddIpAddressFailed(AddIpAddressFailedEventArgs e)
        {
            AddIpAddressFailed?.Invoke(this, e);
        }

        private void OnBlacklistUpdated(EventArgs e)
        {
            BlacklistUpdated?.Invoke(this, e);
        }

        private void OnGotLatestBlacklist(GotLatestListEventArgs e)
        {
            GotLatestBlacklist?.Invoke(this, e);
        }

        private void OnGetBlacklistFailed(GetLatestListFailedEventArgs e)
        {
            GetLatestBlacklistFailed?.Invoke(this, e);
        }

        private void OnCouldNotParseIpAddress(CouldNotParseIpAddressEventArgs e)
        {
            CouldNotParseIpAddress?.Invoke(this, e);
        }
    }
}