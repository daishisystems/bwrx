using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Bwrx.Api
{
    public class Blacklist
    {
        private static readonly Lazy<Blacklist> Lazy = new Lazy<Blacklist>(() => new Blacklist());

        private readonly ConcurrentBag<IPAddress> _blacklist;

        public Blacklist()
        {
            _blacklist = new ConcurrentBag<IPAddress>();
        }

        public static Blacklist Instance => Lazy.Value;

        public event EventHandlers.IpAddressAddedToBlacklistHandler IpAddressAdded;

        public bool AddIPAddress(
            IPAddress ipAddress,
            IEnumerable<IPAddress> whitelistedIPAddresses)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
            if (whitelistedIPAddresses == null) throw new ArgumentNullException(nameof(whitelistedIPAddresses));

            if (whitelistedIPAddresses.Contains(ipAddress) || _blacklist.Contains(ipAddress)) return false;
            _blacklist.Add(ipAddress);
            OnIpAddressAdded(new IpAddressAddedToBlacklistEventArgs(ipAddress));
            return true;
        }

        private void OnIpAddressAdded(IpAddressAddedToBlacklistEventArgs e)
        {
            IpAddressAdded?.Invoke(this, e);
        }
    }
}