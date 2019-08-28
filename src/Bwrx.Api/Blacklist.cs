using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NetTools;

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

        public IEnumerable<IPAddressRange> IpAddressRanges { get; set; }

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

        public bool IpAddressIsInRange(string ipAddress, string ipAddressRange)
        {
            if (string.IsNullOrEmpty(ipAddress)) throw new ArgumentNullException(nameof(ipAddress));
            if (string.IsNullOrEmpty(ipAddressRange)) throw new ArgumentNullException(nameof(ipAddressRange));

            var canParseIpAddress = IPAddress.TryParse(ipAddress, out var ip);
            if (!canParseIpAddress) throw new Exception("Could not parse IP address '" + ipAddress + "'");

            var canParseIpAddressRange = IPAddressRange.TryParse(ipAddressRange, out var ipRange);
            if (canParseIpAddressRange)
                return ipRange.Contains(ip);
            throw new Exception("Could not parse IP address range '" + ipAddressRange + "'");
        }

        // todo: Consider adding notification events
        public bool IpAddressIsInRanges(string ipAddress, List<string> ipAddressRanges, out int ipRangeIndex)
        {
            if (string.IsNullOrEmpty(ipAddress)) throw new ArgumentNullException(nameof(ipAddress));
            if (ipAddressRanges == null) throw new ArgumentNullException(nameof(ipAddressRanges));

            if (!ipAddressRanges.Any())
            {
                ipRangeIndex = 0;
                return false;
            }

            var isInRange = false;
            var counter = 0;
            do
            {
                var ipRange = ipAddressRanges[counter];
                if (IpAddressIsInRange(ipAddress, ipRange)) isInRange = true;
                counter++;
            } while (!isInRange && counter < ipAddressRanges.Count);

            ipRangeIndex = counter - 1;
            return isInRange;
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