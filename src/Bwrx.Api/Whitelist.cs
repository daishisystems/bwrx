using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;

namespace Bwrx.Api
{
    public class Whitelist
    {
        private static readonly Lazy<Whitelist> Lazy = new Lazy<Whitelist>(() => new Whitelist());

        public Whitelist()
        {
            IpAddresses = new ConcurrentBag<IPAddress>();
        }

        public static Whitelist Instance => Lazy.Value;

        public ConcurrentBag<IPAddress> IpAddresses { get; private set; }

        public event EventHandlers.IpAddressAddedHandler IpAddressAdded;

        public event EventHandlers.AddIpAddressFailedEventHandler AddIpAddressFailed;

        public event EventHandlers.ListUpdatedHandler WhitelistUpdated;

        public event EventHandlers.GotLatestListEventHandler GotLatestWhitelist;

        public event EventHandlers.GetLatestListFailedEventHandler GetLatestWhitelistFailed;

        public event EventHandlers.CouldNotParseIpAddressEventHandler CouldNotParseIpAddress;

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

        public void UpDate(IEnumerable<IPAddress> whiteListedIPAddresses)
        {
            IpAddresses = new ConcurrentBag<IPAddress>(whiteListedIPAddresses);
            OnWhitelistUpdated(new EventArgs());
        }

        public async Task<IEnumerable<IPAddress>> GetLatestAsync(BigQueryClient bigQueryClient) // todo: Abstract this
        {
            if (bigQueryClient == null) throw new ArgumentNullException(nameof(bigQueryClient));

            const string getWhitelistQuery = @"SELECT
                  ipaddress
                FROM
                  ipaddress_lists.whitelist
                WHERE
                  _PARTITIONTIME BETWEEN TIMESTAMP_TRUNC(TIMESTAMP_SUB(CURRENT_TIMESTAMP(), INTERVAL 2 * 24 HOUR),DAY)
                  AND TIMESTAMP_TRUNC(CURRENT_TIMESTAMP(),DAY);";

            var whiteList = new List<IPAddress>();
            try
            {
                var data = await bigQueryClient.ExecuteQueryAsync(getWhitelistQuery, null);

                foreach (var row in data)
                {
                    const string ipAddressMetaName = "ipaddress";
                    var ipAddressMeta = row[ipAddressMetaName].ToString();

                    var canParse = IPAddress.TryParse(ipAddressMeta, out var ipAddress);
                    if (canParse)
                        whiteList.Add(ipAddress);
                    else
                        OnCouldNotParseIpAddress(new CouldNotParseIpAddressEventArgs(ipAddressMeta));
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