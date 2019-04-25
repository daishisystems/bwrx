using System;

namespace Bwrx.Api
{
    public class EventTransmittedEventArgs : EventArgs
    {
        public EventTransmittedEventArgs(int numItemsTransmitted)
        {
            NumItemsTransmitted = numItemsTransmitted;
        }

        public int NumItemsTransmitted { get; }
    }
}