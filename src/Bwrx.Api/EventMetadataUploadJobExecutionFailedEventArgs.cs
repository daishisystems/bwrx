using System;

namespace Bwrx.Api
{
    public class EventMetadataUploadJobExecutionFailedEventArgs : EventArgs
    {
        public EventMetadataUploadJobExecutionFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}