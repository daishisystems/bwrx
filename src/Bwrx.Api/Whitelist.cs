using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bwrx.Api
{
    public class Whitelist
    {
        private static readonly Lazy<Whitelist> Lazy = new Lazy<Whitelist>(() => new Whitelist());

        private HttpClient _httpClient;
        private volatile bool _initialised;
        private string _ipAddressesTableName;
        private string _ipAddressRangesTableName;
        private int _maxNumIpAddressesPerHttpRequest;

        private string _whiteListCountUri;
        private string _whitelistRangesUri;
        private string _whiteListUri;

        public static Whitelist Instance => Lazy.Value;

        public HashSet<string> IpAddresses { get; private set; } = new HashSet<string>();
        public List<string> IpAddressRanges { get; private set; } = new List<string>();

        public event EventHandlers.IpAddressAddedHandler IpAddressAdded;

        public event EventHandlers.AddIpAddressFailedEventHandler AddIpAddressFailed;

        public event EventHandlers.ListUpdatedHandler WhitelistUpdated;

        public event EventHandlers.GotLatestListEventHandler GotLatestWhitelist;

        public event EventHandlers.GetLatestListFailedEventHandler GetLatestWhitelistFailed;

        public event EventHandlers.CouldNotParseIpAddressEventHandler CouldNotParseIpAddress;

        public void Init(ClientConfigSettings clientConfigSettings)
        {
            if (_initialised) return;
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
            _whitelistRangesUri = clientConfigSettings.WhitelistRangesUri;
            _maxNumIpAddressesPerHttpRequest = clientConfigSettings.MaxNumIpAddressesPerHttpRequest;
            _ipAddressesTableName = clientConfigSettings.WhitelistIPAddressesTableName;
            _ipAddressRangesTableName = clientConfigSettings.WhitelistIPAddressRangesTableName;
            _initialised = true;
        }

        public void UpDate(IEnumerable<string> ipAddresses, IEnumerable<string> ipAddressRanges)
        {
            IpAddresses = new HashSet<string>(ipAddresses);
            IpAddressRanges = new List<string>(ipAddressRanges);
            OnWhitelistUpdated(new EventArgs());
        }

        public async Task<HashSet<string>> GetLatestIndividualAsync()
        {
            try
            {
                var bulkDataDownloader = new BulkDataDownloader();
                var recordCount =
                    await bulkDataDownloader.GetRecordCountAsync(_httpClient,
                        _whiteListCountUri +
                        "?tablename=" + _ipAddressesTableName);
                if (recordCount.Total == 0) return new HashSet<string>();

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

        public async Task<List<string>> GetLatestRangesAsync()
        {
            try
            {
                var bulkDataDownloader = new BulkDataDownloader();
                var recordCount =
                    await bulkDataDownloader.GetRecordCountAsync(_httpClient,
                        _whiteListCountUri + "?tablename=" + _ipAddressRangesTableName);
                if (recordCount.Total == 0) return new List<string>();

                var numHttpRequestsRequired =
                    bulkDataDownloader.CalcNumHttpRequestsRequired(recordCount.Total, _maxNumIpAddressesPerHttpRequest);
                var paginationSequence = bulkDataDownloader.CalcPaginationSequence(
                    numHttpRequestsRequired,
                    _maxNumIpAddressesPerHttpRequest);
                var data = await bulkDataDownloader.LoadDataAsync<IpAddressRangeMeta>(_httpClient, _whitelistRangesUri,
                    paginationSequence);

                var whitelistranges = data.Select(ipAddressMeta => ipAddressMeta.IpAddressRange).ToList();
                OnGotLatestWhitelist(new GotLatestListEventArgs(whitelistranges.Count));
                return whitelistranges;
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not get the latest whitelist ranges.";
                OnGetLatestWhitelistFailed(new GetLatestListFailedEventArgs(new Exception(errorMessage, exception)));
                return new List<string>();
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