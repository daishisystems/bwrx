using System;

namespace Bwrx.Api
{
    public class RecordNewRelicCustomEventFailedEventArgs : EventArgs
    {
        public RecordNewRelicCustomEventFailedEventArgs(string eventType, Exception exception)
        {
            EventType = eventType;
            Exception = exception;
        }

        public string EventType { get; set; }
        public Exception Exception { get; set; }
    }
}