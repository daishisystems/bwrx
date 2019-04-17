using System;
using System.Net;

namespace Bwrx.Api
{
    public class IpAddressAddedToBlacklistEventArgs : EventArgs
    {
        public IpAddressAddedToBlacklistEventArgs(IPAddress ipAddress)
        {
            IPAddress = ipAddress;            
        }

        public IPAddress IPAddress { get; }        
    }
}