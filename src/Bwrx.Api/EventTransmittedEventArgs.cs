using System;

namespace Bwrx.Api
{
    public class EventTransmittedEventArgs : EventArgs
    {
        public EventTransmittedEventArgs(int numItemsTransmitted)
        {
            NumItemsTransferred = numItemsTransmitted;
        }

        public int NumItemsTransferred { get; }
    }
}