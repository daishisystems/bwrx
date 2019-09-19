using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bwrx.Api
{
    public class EventTransmissionClient
    {
        private static readonly Lazy<EventTransmissionClient> InnerEventTransmissionClient =
            new Lazy<EventTransmissionClient>(() => new EventTransmissionClient());

        private HttpClient _httpClient;
        private volatile bool _initialised;

        private PublisherClient _publisher;

        public static EventTransmissionClient Instance => InnerEventTransmissionClient.Value;

        public event EventHandlers.InitialisationFailedEventHandler InitialisationFailed;

        public event EventHandlers.TransmissionFailedEventHandler TransmissionFailed;

        public event EventHandlers.DataTransmittedEventHandler DataTransmitted;

        public async Task InitAsync(
            CloudServiceCredentials cloudServiceCredentials,
            ClientConfigSettings clientConfigSettings)
        {
            if (_initialised) return;
            if (cloudServiceCredentials == null)
                throw new ArgumentNullException(nameof(cloudServiceCredentials));
            if (clientConfigSettings == null)
                throw new ArgumentNullException(nameof(clientConfigSettings));
            if (string.IsNullOrEmpty(clientConfigSettings.ProjectId))
                throw new ArgumentNullException(nameof(clientConfigSettings.ProjectId));
            if (string.IsNullOrEmpty(clientConfigSettings.CloudFunctionHttpBaseAddress))
                throw new ArgumentNullException(nameof(clientConfigSettings.CloudFunctionHttpBaseAddress));

            try
            {
                if (clientConfigSettings.UsegRpc)
                    await InitPubSub(cloudServiceCredentials, clientConfigSettings);
                else
                    InitHttp(clientConfigSettings);

                _initialised = true;
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
            if (_initialised) return;
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

                _initialised = true;
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while initializing the data transmission client.";
                OnInitialisationFailed(
                    new EventTransmissionClientInitialisationFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }

        public async Task TransmitOvergRpcAsync(IEnumerable<string> eventMetadataPayloadBatch)
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

        public async Task TransmitOverHttpAsync(IEnumerable<string> eventMetadataPayloadBatch, string requestUri)
        {
            if (eventMetadataPayloadBatch == null)
                throw new ArgumentNullException(nameof(eventMetadataPayloadBatch));
            try
            {
                var eventMeta = eventMetadataPayloadBatch.ToList();
                if (!eventMeta.Any()) return;

                var jArray = new JArray();
                foreach (var jToken in eventMeta.Select(JToken.Parse)) jArray.Add(jToken);
                await _httpClient.PostAsync(requestUri, // todo: throw error if response code != 200
                    new StringContent(jArray.ToString(), Encoding.UTF8, "application/json"));

                OnDataTransmitted(new EventTransmittedEventArgs(eventMeta.Count));
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while transmitting the payload.";
                OnTransmissionFailed(
                    new EventTransmissionFailedEventArgs(new Exception(errorMessage,
                        exception)));
            }
        }

        private async Task InitPubSub(CloudServiceCredentials cloudServiceCredentials,
            ClientConfigSettings clientConfigSettings)
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
        }

        private void InitHttp(ClientConfigSettings clientConfigSettings)
        {
            if (!string.IsNullOrEmpty(clientConfigSettings.HttpProxy))
            {
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = new WebProxy(clientConfigSettings.HttpProxy),
                    UseProxy = true
                };
                _httpClient = new HttpClient(httpClientHandler)
                {
                    BaseAddress = new Uri(clientConfigSettings.CloudFunctionHttpBaseAddress)
                };
            }
            else
            {
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(clientConfigSettings.CloudFunctionHttpBaseAddress)
                };
            }
        }

        private static Task<SubscriberClient.Reply> GetEvent(
            PubsubMessage msg,
            CancellationToken cancellationToken)
        {
            try
            {
                return Task.FromResult(SubscriberClient.Reply.Ack);
            }
            catch
            {
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