using System;

namespace Bwrx.Api
{
    public class IPAddressRangeCheckFailedEventArgs : EventArgs
    {
        public IPAddressRangeCheckFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}