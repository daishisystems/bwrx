using System;
using System.Net;

namespace Bwrx.Api
{
    public class IpAddressAddedEventArgs : EventArgs
    {
        public IpAddressAddedEventArgs(IPAddress ipAddress)
        {
            IPAddress = ipAddress;
        }

        public IPAddress IPAddress { get; }
    }
}