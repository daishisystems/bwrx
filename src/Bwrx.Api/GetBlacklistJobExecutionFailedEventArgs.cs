using System;

namespace Bwrx.Api
{
    public class GetBlacklistJobExecutionFailedEventArgs : EventArgs
    {
        public GetBlacklistJobExecutionFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}