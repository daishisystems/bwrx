using System;

namespace Bwrx.Api
{
    public class EventMetadataPublishJobExecutionFailedEventArgs : EventArgs
    {
        public EventMetadataPublishJobExecutionFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}