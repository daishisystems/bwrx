using System;

namespace Bwrx.Api
{
    public class CloudDatabaseConnectionFailedEventArgs : EventArgs
    {
        public CloudDatabaseConnectionFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}