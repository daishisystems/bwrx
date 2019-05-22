using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;
using Newtonsoft.Json;

namespace Bwrx.Api
{
    public class Whitelist
    {
        private static readonly Lazy<Whitelist> Lazy = new Lazy<Whitelist>(() => new Whitelist());

        private HttpClient _httpClient;

        public Whitelist()
        {
            IpAddresses = new ConcurrentBag<IPAddress>();
            IpAddressIndex = new HashSet<IPAddress>();
        }

        public static Whitelist Instance => Lazy.Value;

        public ConcurrentBag<IPAddress> IpAddresses { get; private set; }
        public HashSet<IPAddress> IpAddressIndex { get; private set; }

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
        }

        public bool AddIPAddress(IPAddress ipAddress)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));

            try
            {
                if (IpAddresses.Contains(ipAddress)) return false;
                IpAddresses.Add(ipAddress);
                OnIpAddressAdded(new IpAddressAddedEventArgs(ipAddress));
                return true;
            }
            catch (Exception e)
            {
                var exception = new Exception("Failed to add IP address to whitelist.", e);
                OnAddIpAddressFailed(new AddIpAddressFailedEventArgs(exception, ipAddress));
                return false;
            }
        }

        public void UpDate(List<IPAddress> whiteListedIPAddresses)
        {
            IpAddresses = new ConcurrentBag<IPAddress>(whiteListedIPAddresses);
            IpAddressIndex = new HashSet<IPAddress>(whiteListedIPAddresses);
            OnWhitelistUpdated(new EventArgs());
        }

        public async Task<IEnumerable<IPAddress>> GetLatestAsync()
        {
            var whiteList = new List<IPAddress>();
            try
            {
                var httpResponse = await _httpClient.GetStringAsync(string.Empty);
                var rawIpAddresses =
                    JsonConvert.DeserializeObject<IEnumerable<ListIpAddress>>(httpResponse);

                foreach (var rawIpAddress in rawIpAddresses)
                {
                    var canParse = IPAddress.TryParse(rawIpAddress.IpAddress, out var ipAddress);

                    if (canParse)
                        whiteList.Add(ipAddress);
                    else
                        OnCouldNotParseIpAddress(new CouldNotParseIpAddressEventArgs(rawIpAddress.IpAddress));
                }

                OnGotLatestWhitelist(new GotLatestListEventArgs(whiteList.Count));
                return whiteList;
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not get the latest whitelist.";
                OnGetLatestWhitelistFailed(new GetLatestListFailedEventArgs(new Exception(errorMessage, exception)));
                return whiteList;
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