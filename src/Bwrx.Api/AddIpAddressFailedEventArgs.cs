using System;
using System.Net;

namespace Bwrx.Api
{
    public class AddIpAddressFailedEventArgs : EventArgs
    {
        public AddIpAddressFailedEventArgs(Exception exception, IPAddress ipAddress)
        {
            Exception = exception;
            IpAddress = ipAddress;
        }

        public Exception Exception { get; }

        public IPAddress IpAddress { get; set; }
    }
}