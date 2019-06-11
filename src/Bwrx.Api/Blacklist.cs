using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;
using Newtonsoft.Json;

namespace Bwrx.Api
{
    public class Blacklist
    {
        private static readonly Lazy<Blacklist> Lazy = new Lazy<Blacklist>(() => new Blacklist());

        public Blacklist()
        {
            IpAddresses = new ConcurrentBag<IPAddress>();
        }

        private HttpClient _httpClient;

        public static Blacklist Instance => Lazy.Value;

        public ConcurrentBag<IPAddress> IpAddresses { get; private set; }

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
        }
        
        // todo: Use O(1) collection
        // todo: Flatten to IP ranges collection
        public bool IsIpAddressBlacklisted(IPAddress ipAddressToFind)
        {
            return IpAddresses.Contains(ipAddressToFind);
        }

        public bool AddIPAddress(
            IPAddress ipAddress,
            IEnumerable<IPAddress> whitelistedIPAddresses = null)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));

            try
            {
                if (whitelistedIPAddresses != null && whitelistedIPAddresses.Contains(ipAddress)) return false;
                if (IpAddresses.Contains(ipAddress)) return false;
                IpAddresses.Add(ipAddress);
                OnIpAddressAdded(new IpAddressAddedEventArgs(ipAddress));
                return true;
            }
            catch (Exception e)
            {
                var exception = new Exception("Failed to add IP address to blacklist.", e);
                OnAddIpAddressFailed(new AddIpAddressFailedEventArgs(exception, ipAddress));
                return false;
            }
        }

        public void UpDate(IEnumerable<IPAddress> blacklistedIPAddresses)
        {
            IpAddresses = new ConcurrentBag<IPAddress>(blacklistedIPAddresses);
            OnBlacklistUpdated(new EventArgs());
        }

        public async Task<IEnumerable<IPAddress>> GetLatestAsync(
            HashSet<IPAddress> whitelistedIpAddresses)
        {
            if (whitelistedIpAddresses == null) throw new ArgumentNullException(nameof(whitelistedIpAddresses));

            var blacklist = new List<IPAddress>();
            try
            {
                var httpResponse = await _httpClient.GetStringAsync(string.Empty);
                var rawIpAddresses =
                    JsonConvert.DeserializeObject<IEnumerable<ListIpAddress>>(httpResponse);

                var ipAddresses = new List<IPAddress>();
                foreach (var rawIpAddress in rawIpAddresses)
                {
                    var canParse = IPAddress.TryParse(rawIpAddress.IpAddress, out var ipAddress);

                    if (canParse)
                        ipAddresses.Add(ipAddress);
                    else
                        OnCouldNotParseIpAddress(new CouldNotParseIpAddressEventArgs(rawIpAddress.IpAddress));
                }

                blacklist.AddRange(ipAddresses.Where(ipAddress => !whitelistedIpAddresses.Contains(ipAddress)));

                OnGotLatestBlacklist(new GotLatestListEventArgs(blacklist.Count));
                return blacklist;
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not get the latest blacklist.";
                OnGetBlacklistFailed(new GetLatestListFailedEventArgs(new Exception(errorMessage, exception)));
                return blacklist;
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