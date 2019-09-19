using System;

namespace Bwrx.Api
{
    public class JobSchedulerShutdownFailedEventArgs : EventArgs
    {
        public JobSchedulerShutdownFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}