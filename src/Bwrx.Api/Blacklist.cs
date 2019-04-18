﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
    }
}