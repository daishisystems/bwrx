using System;
using System.Collections.Generic;

namespace Bwrx.Api
{
    public class CouldNotParseIpAddressHttpHeaderValuesEventArgs : EventArgs
    {
        public CouldNotParseIpAddressHttpHeaderValuesEventArgs(
            IEnumerable<string> ipAddressHttpHeaderValues,
            Exception exception = null)
        {
            IpAddressHttpHeaderValues = ipAddressHttpHeaderValues;
            Exception = exception;
        }

        public IEnumerable<string> IpAddressHttpHeaderValues { get; }

        public Exception Exception { get; set; }
    }
}