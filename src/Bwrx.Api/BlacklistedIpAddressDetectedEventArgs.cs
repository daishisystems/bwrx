using System;
using System.Collections.Generic;
using System.Net;

namespace Bwrx.Api
{
    public class BlacklistedIpAddressDetectedEventArgs : EventArgs
    {
        public BlacklistedIpAddressDetectedEventArgs(IEnumerable<IPAddress> ipAddresses, bool passiveMode)
        {
            IPAddresses = ipAddresses;
            PassiveMode = passiveMode;
        }

        public IEnumerable<IPAddress> IPAddresses { get; }
        public bool PassiveMode { get; }
    }
}