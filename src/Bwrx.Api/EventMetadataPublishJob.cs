using System;
using System.Threading.Tasks;
using Quartz;

namespace Bwrx.Api
{
    internal class EventMetadataPublishJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var eventMetaCache = (EventMetaCache) dataMap[nameof(EventMetaCache)];
                var eventTransmissionClient = (EventTransmissionClient) dataMap[nameof(EventTransmissionClient)];
                var clientConfigSettings = (ClientConfigSettings) dataMap[nameof(ClientConfigSettings)];

                var eventMetadataPayloadBatch = eventMetaCache.GetEventMetadataPayloadBatch();

                if (clientConfigSettings.UsegRpc)
                    await eventTransmissionClient.TransmitOvergRpcAsync(eventMetadataPayloadBatch);
                else
                    await eventTransmissionClient.TransmitOverHttpAsync(
                        eventMetadataPayloadBatch,
                        clientConfigSettings.CloudFunctionRequestUri); // todo: Parse types from the payload and redirect appropriately ...
            }
            catch (Exception exception)
            {
                const string errorMessage = "Event meta-upload background task execution failed.";
                throw new Exception(errorMessage, exception);
            }
        }
    }
}