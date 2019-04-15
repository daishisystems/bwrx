using System;

namespace Bwrx.Api
{
    public class EventMetaAddedEventArgs : EventArgs
    {
        public EventMetaAddedEventArgs(object eventMeta)
        {
            EventMeta = eventMeta;
        }

        public object EventMeta { get; }
    }
}