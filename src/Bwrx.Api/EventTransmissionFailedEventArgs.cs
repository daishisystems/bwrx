using System;

namespace Bwrx.Api
{
    public class EventTransmissionFailedEventArgs : EventArgs
    {
        public EventTransmissionFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}