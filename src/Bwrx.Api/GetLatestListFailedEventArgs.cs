using System;

namespace Bwrx.Api
{
    public class GetLatestListFailedEventArgs : EventArgs
    {
        public GetLatestListFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}