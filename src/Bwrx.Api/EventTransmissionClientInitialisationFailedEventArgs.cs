using System;

namespace Bwrx.Api
{
    public class EventTransmissionClientInitialisationFailedEventArgs : EventArgs
    {
        public EventTransmissionClientInitialisationFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}