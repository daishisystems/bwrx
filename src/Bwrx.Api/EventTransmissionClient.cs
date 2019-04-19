using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Auth;
using Grpc.Core;
using Newtonsoft.Json;

namespace Bwrx.Api
{
    public class EventTransmissionClient
    {
        private static readonly Lazy<EventTransmissionClient> InnerEventTransmissionClient =
            new Lazy<EventTransmissionClient>(() => new EventTransmissionClient());

        private PublisherClient _publisher;
        private SubscriberClient _subscriber;

        public static EventTransmissionClient Instance => InnerEventTransmissionClient.Value;

        public bool Initialised { get; private set; }

        public event EventHandlers.InitialisationFailedEventHandler InitialisationFailed;

        public event EventHandlers.TransmissionFailedEventHandler TransmissionFailed;

        public event EventHandlers.DataTransmittedEventHandler DataTransmitted;

        public async Task InitAsync(
            CloudServiceCredentials cloudServiceCredentials,
            ClientConfigSettings clientConfigSettings)
        {
            if (cloudServiceCredentials == null)
                throw new ArgumentNullException(nameof(cloudServiceCredentials));
            if (clientConfigSettings == null)
                throw new ArgumentNullException(nameof(clientConfigSettings));
            if (string.IsNullOrEmpty(clientConfigSettings.ProjectId))
                throw new ArgumentNullException(nameof(clientConfigSettings.ProjectId));
            if (string.IsNullOrEmpty(clientConfigSettings.PublisherTopicId))
                throw new ArgumentNullException(nameof(clientConfigSettings.PublisherTopicId));

            try
            {
                var credential = GoogleCredential
                    .FromJson(JsonConvert.SerializeObject(cloudServiceCredentials))
                    .CreateScoped(PublisherServiceApiClient.DefaultScopes);

                var publisherSettings = new PublisherClient.Settings
                {
                    BatchingSettings = new BatchingSettings(
                        clientConfigSettings.ElementCountThreshold,
                        clientConfigSettings.RequestByteThreshold,
                        TimeSpan.FromSeconds(clientConfigSettings.EventPublishDelayThreshold))
                };

                var publisherClientCreationSettings = new PublisherClient.ClientCreationSettings(
                    null,
                    null,
                    credential.ToChannelCredentials());

                var publisherTopicName = new TopicName(
                    clientConfigSettings.ProjectId,
                    clientConfigSettings.PublisherTopicId);

                _publisher = await PublisherClient.CreateAsync(
                    publisherTopicName,
                    publisherClientCreationSettings,
                    publisherSettings);

                Initialised = true;
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while initializing the data transmission client.";
                OnInitialisationFailed(
                    new EventTransmissionClientInitialisationFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }

        public async Task InitAsync(
            CloudServiceCredentials cloudServiceCredentials,
            ClientConfigSettings clientConfigSettings,
            string subscriptionId)
        {
            if (cloudServiceCredentials == null)
                throw new ArgumentNullException(nameof(cloudServiceCredentials));
            if (clientConfigSettings == null)
                throw new ArgumentNullException(nameof(clientConfigSettings));
            if (string.IsNullOrEmpty(clientConfigSettings.ProjectId))
                throw new ArgumentNullException(nameof(clientConfigSettings.ProjectId));
            if (string.IsNullOrEmpty(clientConfigSettings.PublisherTopicId))
                throw new ArgumentNullException(nameof(clientConfigSettings.PublisherTopicId));
            if (string.IsNullOrEmpty(subscriptionId))
                throw new ArgumentNullException(nameof(subscriptionId));

            try
            {
                var credential = GoogleCredential
                    .FromJson(JsonConvert.SerializeObject(cloudServiceCredentials))
                    .CreateScoped(PublisherServiceApiClient.DefaultScopes);

                var publisherSettings = new PublisherClient.Settings
                {
                    BatchingSettings = new BatchingSettings(
                        clientConfigSettings.ElementCountThreshold,
                        clientConfigSettings.RequestByteThreshold,
                        TimeSpan.FromSeconds(clientConfigSettings.EventPublishDelayThreshold))
                };

                var publisherClientCreationSettings = new PublisherClient.ClientCreationSettings(
                    null,
                    null,
                    credential.ToChannelCredentials());

                var publisherTopicName = new TopicName(
                    clientConfigSettings.ProjectId,
                    clientConfigSettings.PublisherTopicId);

                _publisher = await PublisherClient.CreateAsync(
                    publisherTopicName,
                    publisherClientCreationSettings,
                    publisherSettings);

                var subscriberTopicName = new TopicName(
                    clientConfigSettings.ProjectId,
                    clientConfigSettings.SubscriberTopicId);

                var subscriptionName = new SubscriptionName(clientConfigSettings.ProjectId, subscriptionId);
                try
                {
                    var channel = new Channel(
                        SubscriberServiceApiClient.DefaultEndpoint.Host,
                        SubscriberServiceApiClient.DefaultEndpoint.Port, credential.ToChannelCredentials());
                    var client = SubscriberServiceApiClient.Create(channel);
                    client.CreateSubscription(subscriptionName, subscriberTopicName, null, null);
                }
                catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists)
                {
                    // ignored
                }

                var subscriberClientCreationSettings = new SubscriberClient.ClientCreationSettings(
                    null,
                    null,
                    credential.ToChannelCredentials());

                _subscriber = await SubscriberClient.CreateAsync(subscriptionName, subscriberClientCreationSettings);
                Initialised = true;
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while initializing the data transmission client.";
                OnInitialisationFailed(
                    new EventTransmissionClientInitialisationFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }

        public async Task TransmitAsync(IEnumerable<string> eventMetadataPayloadBatch)
        {
            if (eventMetadataPayloadBatch == null)
                throw new ArgumentNullException(nameof(eventMetadataPayloadBatch));
            try
            {
                var publishTasks = eventMetadataPayloadBatch
                    .Select(eventMetadataPayload => new PubsubMessage
                        {Data = ByteString.CopyFromUtf8(eventMetadataPayload)})
                    .Select(pubsubMessage => _publisher.PublishAsync(pubsubMessage)).ToList();

                foreach (var publishTask in publishTasks) await publishTask;

                OnDataTransmitted(new EventTransmittedEventArgs(publishTasks.Count));
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while transmitting the payload.";
                OnTransmissionFailed(
                    new EventTransmissionFailedEventArgs(new Exception(errorMessage,
                        exception)));
            }
        }

        public async Task SubscribeAsync()
        {
            await _subscriber.StartAsync(GetEvent);
        }

        private static Task<SubscriberClient.Reply> GetEvent(
            PubsubMessage msg,
            CancellationToken cancellationToken)
        {
            try
            {
                // todo: new handler
                return Task.FromResult(SubscriberClient.Reply.Ack);
            }
            catch
            {
                // todo: new handler
                return null;
            }
        }

        private void OnTransmissionFailed(EventTransmissionFailedEventArgs e)
        {
            TransmissionFailed?.Invoke(this, e);
        }

        private void OnInitialisationFailed(EventTransmissionClientInitialisationFailedEventArgs e)
        {
            InitialisationFailed?.Invoke(this, e);
        }

        private void OnDataTransmitted(EventTransmittedEventArgs e)
        {
            DataTransmitted?.Invoke(this, e);
        }
    }
}