using System;

namespace Bwrx.Api
{
    public class CouldNotGetIpAddressHttpHeaderValuesEventArgs : EventArgs
    {
        public CouldNotGetIpAddressHttpHeaderValuesEventArgs(
            string ipAddressHeaderName,
            Exception exception = null)
        {
            IPAddressHeaderName = ipAddressHeaderName;
            Exception = exception;
        }

        public string IPAddressHeaderName { get; }

        public Exception Exception { get; set; }
    }
}