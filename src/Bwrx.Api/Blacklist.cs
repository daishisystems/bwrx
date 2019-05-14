using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;

namespace Bwrx.Api
{
    public class Blacklist
    {
        private static readonly Lazy<Blacklist> Lazy = new Lazy<Blacklist>(() => new Blacklist());

        public Blacklist()
        {
            IpAddresses = new ConcurrentBag<IPAddress>();
        }

        public static Blacklist Instance => Lazy.Value;

        public ConcurrentBag<IPAddress> IpAddresses { get; private set; }

        public event EventHandlers.IpAddressAddedHandler IpAddressAdded;

        public event EventHandlers.AddIpAddressFailedEventHandler AddIpAddressFailed;

        public event EventHandlers.ListUpdatedHandler BlacklistUpdated;

        public event EventHandlers.GotLatestListEventHandler GotLatestBlacklist;

        public event EventHandlers.GetLatestListFailedEventHandler GetLatestBlacklistFailed;

        public event EventHandlers.CouldNotParseIpAddressEventHandler CouldNotParseIpAddress;

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
            BigQueryClient bigQueryClient,
            HashSet<IPAddress> whitelistedIpAddresses,
            int blacklistPartitionIntervalDays)
        {
            if (bigQueryClient == null) throw new ArgumentNullException(nameof(bigQueryClient));
            if (whitelistedIpAddresses == null) throw new ArgumentNullException(nameof(whitelistedIpAddresses));

            var getBlacklistQuery = @"SELECT
                  ipaddress
                FROM
                  ipaddress_lists.blacklist
                WHERE
                  _PARTITIONTIME BETWEEN TIMESTAMP_TRUNC(TIMESTAMP_SUB(CURRENT_TIMESTAMP(), INTERVAL " +
                                    blacklistPartitionIntervalDays + @" * 24 HOUR),DAY)
                  AND TIMESTAMP_TRUNC(CURRENT_TIMESTAMP(),DAY);";

            var blacklist = new List<IPAddress>();
            try
            {
                var data = await bigQueryClient.ExecuteQueryAsync(getBlacklistQuery, null);

                foreach (var row in data)
                {
                    const string ipAddressMetaName = "ipaddress";
                    var ipAddressMeta = row[ipAddressMetaName].ToString();

                    var canParse = IPAddress.TryParse(ipAddressMeta, out var ipAddress);
                    if (canParse)
                    {
                        if (!whitelistedIpAddresses.Contains(ipAddress)) blacklist.Add(ipAddress);
                    }
                    else
                    {
                        OnCouldNotParseIpAddress(new CouldNotParseIpAddressEventArgs(ipAddressMeta));
                    }
                }

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