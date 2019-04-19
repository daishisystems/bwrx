#if NET461
using Google.Apis.Auth.OAuth2;
using System.Net;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Specialized;
#endif
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if (NETCOREAPP2_1 || NETCOREAPP2_2)
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

#endif

namespace Bwrx.Api
{
    public class Agent
    {
        private static readonly Lazy<Agent> Lazy =
            new Lazy<Agent>(() => new Agent());

        public event EventHandlers.CloudDatabaseConnectionFailedEventHandler CloudDatabaseConnectionFailed;

        public static Agent Instance => Lazy.Value;

        private BigQueryClient _bigQueryClient;

        public event EventHandlers.EventMetaAddedEventHandler EventMetaAdded;
        public event EventHandlers.AddEventMetaFailedEventHandler AddEventMetaFailed;
        public event EventHandlers.GetEventMetadataPayloadBatchFailedEventHandler GetEventMetadataPayloadBatchFailed;
        public event EventHandlers.GetEventMetadataPayloadBatchEventHandler GotEventMetadataPayloadBatch;
        public event EventHandlers.ClearCacheFailedEventHandler ClearCacheFailed;

        public event EventHandlers.InitialisationFailedEventHandler InitialisationFailed;
        public event EventHandlers.TransmissionFailedEventHandler TransmissionFailed;
        public event EventHandlers.DataTransmittedEventHandler DataTransmitted;

        public event JobScheduler.JobSchedulerStartFailedEventHandler JobSchedulerStartFailed;
        public event EventMetadataPublishJobExecutionFailedEventHandler EventMetadataPublishJobExecutionFailed;
        public event GetBlacklistJobExecutionFailedEventHandler GetBlacklistJobExecutionFailed;
        public event GetWhitelistJobExecutionFailedEventHandler GetWhitelistJobExecutionFailed;

        public event EventHandlers.IpAddressAddedHandler BlacklistIpAddressAdded;
        public event EventHandlers.AddIpAddressFailedEventHandler BlacklistAddIpAddressFailed;
        public event EventHandlers.ListUpdatedHandler BlacklistListUpdated;
        public event EventHandlers.GotLatestListEventHandler BlacklistGotLatestList;
        public event EventHandlers.GetLatestListFailedEventHandler BlacklistGetLatestListFailed;
        public event EventHandlers.CouldNotParseIpAddressEventHandler BlacklistCouldNotParseIpAddress;

        public event EventHandlers.IpAddressAddedHandler WhitelistIpAddressAdded;
        public event EventHandlers.AddIpAddressFailedEventHandler WhitelistAddIpAddressFailed;
        public event EventHandlers.ListUpdatedHandler WhitelistListUpdated;
        public event EventHandlers.GotLatestListEventHandler WhitelistGotLatestList;
        public event EventHandlers.GetLatestListFailedEventHandler WhitelistGetLatestListFailed;
        public event EventHandlers.CouldNotParseIpAddressEventHandler WhitelistCouldNotParseIpAddress;

        public void Start(
            CloudServiceCredentials cloudServiceCredentials,
            ClientConfigSettings clientConfigSettings)
        {
            EventMetaCache.Instance.EventMetaAdded += EventMetaAdded;
            EventMetaCache.Instance.AddEventMetaFailed += AddEventMetaFailed;
            EventMetaCache.Instance.GetEventMetadataPayloadBatchFailed += GetEventMetadataPayloadBatchFailed;
            EventMetaCache.Instance.GotEventMetadataPayloadBatch += GotEventMetadataPayloadBatch;
            EventMetaCache.Instance.ClearCacheFailed += ClearCacheFailed;

            EventTransmissionClient.Instance.InitialisationFailed += InitialisationFailed;
            EventTransmissionClient.Instance.TransmissionFailed += TransmissionFailed;
            EventTransmissionClient.Instance.DataTransmitted += DataTransmitted;

            JobScheduler.Instance.JobSchedulerStartFailed += JobSchedulerStartFailed;
            JobScheduler.Instance.EventMetadataPublishJobExecutionFailed += EventMetadataPublishJobExecutionFailed;
            JobScheduler.Instance.GetBlacklistJobExecutionFailed += GetBlacklistJobExecutionFailed;
            JobScheduler.Instance.GetWhitelistJobExecutionFailed += GetWhitelistJobExecutionFailed;

            Blacklist.Instance.IpAddressAdded += BlacklistIpAddressAdded;
            Blacklist.Instance.AddIpAddressFailed += BlacklistAddIpAddressFailed;
            Blacklist.Instance.BlacklistUpdated += BlacklistListUpdated;
            Blacklist.Instance.GotLatestBlacklist += BlacklistGotLatestList;
            Blacklist.Instance.GetLatestBlacklistFailed += BlacklistGetLatestListFailed;
            Blacklist.Instance.CouldNotParseIpAddress += BlacklistCouldNotParseIpAddress;

            Whitelist.Instance.IpAddressAdded += WhitelistIpAddressAdded;
            Whitelist.Instance.AddIpAddressFailed += WhitelistAddIpAddressFailed;
            Whitelist.Instance.WhitelistUpdated += WhitelistListUpdated;
            Whitelist.Instance.GotLatestWhitelist += WhitelistGotLatestList;
            Whitelist.Instance.GetLatestWhitelistFailed += WhitelistGetLatestListFailed;
            Whitelist.Instance.CouldNotParseIpAddress += WhitelistCouldNotParseIpAddress;

            EventTransmissionClient.Instance.InitAsync(
                cloudServiceCredentials,
                clientConfigSettings
            ).Wait();

            if (!EventTransmissionClient.Instance.Initialised) return;

            var bigQueryConnectionEstablished = false;
            try
            {
                var googleCredential = GoogleCredential.FromJson(JsonConvert.SerializeObject(cloudServiceCredentials));
                _bigQueryClient = BigQueryClient.Create(clientConfigSettings.ProjectId, googleCredential);
                bigQueryConnectionEstablished = true;
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not establish a connection to the cloud database.";
                OnCloudDatabaseConnectionFailed(
                    new CloudDatabaseConnectionFailedEventArgs(new Exception(errorMessage, exception)));
            }

            if (!bigQueryConnectionEstablished) return;

            JobScheduler.Instance.StartAsync(
                EventTransmissionClient.Instance,
                _bigQueryClient,
                EventMetaCache.Instance,
                Blacklist.Instance,
                Whitelist.Instance,
                clientConfigSettings).Wait();
        }

        public void AddEvent<T>(
            T eventMetadataPayload,
            string eventName,
            string queryString,
            Dictionary<string, string> httpHeaders)
        {
            EventMetaCache.Instance.Add(
                eventMetadataPayload,
                eventName,
                queryString,
                httpHeaders);
        }

        public static string AddTrackingMetadataToJson(
            string json,
            string eventName,
            string queryString,
            Dictionary<string, string> httpHeaders,
            string timestamp)
        {
            try
            {
                var jsonObject = JObject.Parse(json);
                jsonObject.Add(new JProperty("eventName", eventName));
                jsonObject.Add(new JProperty("queryString", queryString));
                jsonObject.Add(httpHeaders != null
                    ? new JProperty("httpHeaders", JObject.FromObject(httpHeaders))
                    : new JProperty("httpHeaders", null));
                jsonObject.Add(new JProperty("created", timestamp));
                return jsonObject.ToString();
            }
            catch (Exception exception)
            {
                throw new JsonSerializationException($"Could not edit the JSON payload: {json}", exception);
            }
        }

        public static bool TryParseIpAddresses(
            IEnumerable<string> ipAddressHttpHeaderValues,
            out IEnumerable<IPAddress> ipAddresses)
        {
            if (ipAddressHttpHeaderValues != null)
            {
                var parsedIpAddresses = new List<IPAddress>();
                var ipAddressRegex = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

                foreach (var ipAddressHttpHeaderValue in ipAddressHttpHeaderValues)
                {
                    var matches = ipAddressRegex.Matches(ipAddressHttpHeaderValue);

                    foreach (Match match in matches)
                    {
                        var isIpAddress = IPAddress.TryParse(match.ToString(), out var ipAddress);
                        if (isIpAddress) parsedIpAddresses.Add(ipAddress);
                    }
                }

                ipAddresses = parsedIpAddresses;
                return parsedIpAddresses.Count > 0;
            }

            ipAddresses = new List<IPAddress>();
            return false;
        }

#if (NETCOREAPP2_1 || NETCOREAPP2_2)
        public static Dictionary<string, string>
            ParseHttpHeaders(IHeaderDictionary httpRequestHeaders)
        {
            return httpRequestHeaders?.ToDictionary(h => h.Key, h => h.Value.LastOrDefault());
        }

        public static string GetFingerprint(
            HttpRequest httpRequest,
            string fingerprintHeaderName = "FingerprintId")
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

            var gotFingerprint = httpRequest.Headers.TryGetValue(fingerprintHeaderName, out var headerValues);
            if (!gotFingerprint) return null;

            string fingerprint = null;
            if (!StringValues.IsNullOrEmpty(headerValues))
                fingerprint = headerValues.LastOrDefault();

            return fingerprint;
        }
#endif

#if NET461
        public static Dictionary<string, string>
            ParseHttpHeaders(NameValueCollection httpRequestHeadersCollection)
        {
            return httpRequestHeadersCollection.Cast<string>()
                .ToDictionary(k => k, k => httpRequestHeadersCollection[k]);
        }

        public static Dictionary<string, string>
            ParseHttpHeaders(HttpRequestHeaders httpRequestHeaders)
        {
            if (httpRequestHeaders == null) return null;

            var parsedHttpHeaders = new Dictionary<string, string>();

            foreach (var httpRequestHeader in httpRequestHeaders)
                parsedHttpHeaders.Add(httpRequestHeader.Key, httpRequestHeader.Value.LastOrDefault());

            return parsedHttpHeaders;
        }

        public static string GetFingerprint(
            HttpRequestMessage httpRequest,
            string fingerprintHeaderName = "FingerprintId")
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (httpRequest.Headers == null) throw new ArgumentNullException(nameof(httpRequest.Headers));

            var gotHeaderValues = httpRequest.Headers.TryGetValues(fingerprintHeaderName, out var headerValues);
            return gotHeaderValues ? headerValues.LastOrDefault() : null;
        }

        public static bool TryGetIpAddressHttpHeaderValues(
            string httpHeaderName,
            HttpRequestMessage httpRequestMessage,
            out IEnumerable<string> ipAddressHttpHeaders)
        {
            if (string.IsNullOrEmpty(httpHeaderName))
            {
                ipAddressHttpHeaders = new List<string>();
                return false;
            }

            if (httpRequestMessage?.Headers != null)
                return httpRequestMessage.Headers.TryGetValues(httpHeaderName, out ipAddressHttpHeaders);

            ipAddressHttpHeaders = new List<string>();
            return false;
        }        
#endif
        private void OnCloudDatabaseConnectionFailed(CloudDatabaseConnectionFailedEventArgs e)
        {
            CloudDatabaseConnectionFailed?.Invoke(this, e);
        }
    }
}