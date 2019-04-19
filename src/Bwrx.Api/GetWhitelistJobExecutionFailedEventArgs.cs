using System;

namespace Bwrx.Api
{
    public class GetWhitelistJobExecutionFailedEventArgs : EventArgs
    {
        public GetWhitelistJobExecutionFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}