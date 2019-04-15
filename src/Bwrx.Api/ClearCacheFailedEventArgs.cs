using System;

namespace Bwrx.Api
{
    public class ClearCacheFailedEventArgs : EventArgs
    {
        public ClearCacheFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}