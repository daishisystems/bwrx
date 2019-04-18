using System;

namespace Bwrx.Api
{
    public class CouldNotParseIpAddressEventArgs : EventArgs
    {
        public CouldNotParseIpAddressEventArgs(string ipAddress)
        {
            IpAddress = ipAddress;
        }

        public string IpAddress { get; }
    }
}