using System;

namespace Bwrx.Api
{
    public class EventHandlers
    {
        public delegate void AddEventMetaFailedEventHandler(object sender, AddEventMetaFailedEventArgs e);

        public delegate void AddIpAddressFailedEventHandler(object sender, AddIpAddressFailedEventArgs e);

        public delegate void ClearCacheFailedEventHandler(object sender, ClearCacheFailedEventArgs e);

        public delegate void CouldNotParseIpAddressEventHandler(object sender, CouldNotParseIpAddressEventArgs e);

        public delegate void DataTransmittedEventHandler(object sender, EventTransmittedEventArgs e);

        public delegate void EventMetaAddedEventHandler(object sender, EventMetaAddedEventArgs e);

        public delegate void GetEventMetadataPayloadBatchEventHandler(
            object sender,
            GetEventMetadataPayloadBatchEventArgs e);

        public delegate void GetEventMetadataPayloadBatchFailedEventHandler(
            object sender,
            GetEventMetadataPayloadBatchFailedEventArgs e);

        public delegate void GetLatestListFailedEventHandler(object sender, GetLatestListFailedEventArgs e);

        public delegate void GotLatestListEventHandler(object sender, GotLatestListEventArgs e);

        public delegate void InitialisationFailedEventHandler(object sender,
            EventTransmissionClientInitialisationFailedEventArgs e);

        public delegate void IpAddressAddedHandler(object sender, IpAddressAddedEventArgs e);

        public delegate void ListUpdatedHandler(object sender, EventArgs e);

        public delegate void TransmissionFailedEventHandler(object sender, EventTransmissionFailedEventArgs e);
    }
}