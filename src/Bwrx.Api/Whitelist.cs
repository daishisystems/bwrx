using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bwrx.Api
{
    public class Whitelist
    {
        private static readonly Lazy<Whitelist> Lazy = new Lazy<Whitelist>(() => new Whitelist());

        private HttpClient _httpClient;
        private int _maxNumIpAddressesPerHttpRequest;
        private string _whiteListCountUri;
        private string _whiteListUri;

        public static Whitelist Instance => Lazy.Value;

        public HashSet<string> IpAddresses { get; private set; } = new HashSet<string>();

        public event EventHandlers.IpAddressAddedHandler IpAddressAdded;

        public event EventHandlers.AddIpAddressFailedEventHandler AddIpAddressFailed;

        public event EventHandlers.ListUpdatedHandler WhitelistUpdated;

        public event EventHandlers.GotLatestListEventHandler GotLatestWhitelist;

        public event EventHandlers.GetLatestListFailedEventHandler GetLatestWhitelistFailed;

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
                    BaseAddress = new Uri(clientConfigSettings.WhitelistUri)
                };
            }
            else
            {
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(clientConfigSettings.WhitelistUri)
                };
            }
            _whiteListUri = clientConfigSettings.WhitelistUri;
            _whiteListCountUri = clientConfigSettings.WhitelistCountUri;
            _maxNumIpAddressesPerHttpRequest = clientConfigSettings.MaxNumIpAddressesPerHttpRequest;
        }

        public void UpDate(IEnumerable<string> ipAddresses)
        {
            IpAddresses = new HashSet<string>(ipAddresses);
            OnWhitelistUpdated(new EventArgs());
        }

        public async Task<HashSet<string>> GetLatestAsync()
        {
            try
            {
                var bulkDataDownloader = new BulkDataDownloader();
                var recordCount =
                    await bulkDataDownloader.GetRecordCountAsync(_httpClient,
                        _whiteListCountUri + "?tablename=whitelist");

                var numHttpRequestsRequired =
                    bulkDataDownloader.CalcNumHttpRequestsRequired(recordCount.Total, _maxNumIpAddressesPerHttpRequest);
                var paginationSequence = bulkDataDownloader.CalcPaginationSequence(
                    numHttpRequestsRequired,
                    _maxNumIpAddressesPerHttpRequest);
                var data = await bulkDataDownloader.LoadDataAsync<IpAddressMeta>(_httpClient, _whiteListUri,
                    paginationSequence);

                var whitelist = new HashSet<string>();
                foreach (var ipAddressMeta in data) whitelist.Add(ipAddressMeta.IpAddress);

                OnGotLatestWhitelist(new GotLatestListEventArgs(whitelist.Count));
                return whitelist;
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not get the latest whitelist.";
                OnGetLatestWhitelistFailed(new GetLatestListFailedEventArgs(new Exception(errorMessage, exception)));
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

        private void OnWhitelistUpdated(EventArgs e)
        {
            WhitelistUpdated?.Invoke(this, e);
        }

        private void OnGotLatestWhitelist(GotLatestListEventArgs e)
        {
            GotLatestWhitelist?.Invoke(this, e);
        }

        private void OnGetLatestWhitelistFailed(GetLatestListFailedEventArgs e)
        {
            GetLatestWhitelistFailed?.Invoke(this, e);
        }

        private void OnCouldNotParseIpAddress(CouldNotParseIpAddressEventArgs e)
        {
            CouldNotParseIpAddress?.Invoke(this, e);
        }
    }
}