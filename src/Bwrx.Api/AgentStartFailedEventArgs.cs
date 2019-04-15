using System;

namespace Bwrx.Api
{
    public class JobSchedulerStartFailedEventArgs : EventArgs
    {
        public JobSchedulerStartFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}