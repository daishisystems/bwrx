#if NET461
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Specialized;
#endif
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Sockets;
using System.Net;

#if (NETCOREAPP2_1 || NETCOREAPP2_2)
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

#endif

namespace Bwrx.Api
{
    public class Agent
    {
        private static readonly Lazy<Agent> Lazy =
            new Lazy<Agent>(() => new Agent());

        private volatile bool _initialised;

        public event EventHandlers.CloudDatabaseConnectionFailedEventHandler CloudDatabaseConnectionFailed;
        public event EventHandlers.RecordNewRelicCustomEventFailedEventHandler RecordNewRelicCustomEventFailed;

        public static Agent Instance => Lazy.Value;

        public ClientConfigSettings ClientConfigSettings { get; private set; }

        public bool Initialised
        {
            get => _initialised;
            private set => _initialised = value;
        }

        public bool HandlerIsInitialised { get; set; }

        public event EventHandlers.EventMetaAddedEventHandler EventMetaAdded;
        public event EventHandlers.AddEventMetaFailedEventHandler AddEventMetaFailed;
        public event EventHandlers.GetEventMetadataPayloadBatchFailedEventHandler GetEventMetadataPayloadBatchFailed;
        public event EventHandlers.GetEventMetadataPayloadBatchEventHandler GotEventMetadataPayloadBatch;
        public event EventHandlers.ClearCacheFailedEventHandler ClearCacheFailed;

        public event EventHandlers.InitialisationFailedEventHandler InitialisationFailed;
        public event EventHandlers.TransmissionFailedEventHandler TransmissionFailed;
        public event EventHandlers.DataTransmittedEventHandler DataTransmitted;

        public event JobScheduler.JobSchedulerStartFailedEventHandler JobSchedulerStartFailed;
        public event JobScheduler.JobSchedulerShutdownFailedEventHandler JobSchedulerShutdownFailed;
        public event EventMetadataPublishJobExecutionFailedEventHandler EventMetadataPublishJobExecutionFailed;
        public event GetBlacklistJobExecutionFailedEventHandler GetBlacklistJobExecutionFailed;
        public event GetWhitelistJobExecutionFailedEventHandler GetWhitelistJobExecutionFailed;

        public event EventHandlers.IpAddressAddedHandler BlacklistIpAddressAdded;
        public event EventHandlers.AddIpAddressFailedEventHandler BlacklistAddIpAddressFailed;
        public event EventHandlers.ListUpdatedHandler BlacklistListUpdated;
        public event EventHandlers.GotLatestListEventHandler BlacklistGotLatestList;
        public event EventHandlers.GotLatestListEventHandler GotLatestBlacklistRanges;
        public event EventHandlers.GetLatestListFailedEventHandler BlacklistGetLatestListFailed;
        public event EventHandlers.CouldNotParseIpAddressEventHandler BlacklistCouldNotParseIpAddress;

        public event EventHandlers.IpAddressAddedHandler WhitelistIpAddressAdded;
        public event EventHandlers.AddIpAddressFailedEventHandler WhitelistAddIpAddressFailed;
        public event EventHandlers.ListUpdatedHandler WhitelistListUpdated;
        public event EventHandlers.GotLatestListEventHandler WhitelistGotLatestList;
        public event EventHandlers.GotLatestListEventHandler WhitelistGotLatestListRanges;
        public event EventHandlers.GetLatestListFailedEventHandler WhitelistGetLatestListFailed;
        public event EventHandlers.CouldNotParseIpAddressEventHandler WhitelistCouldNotParseIpAddress;

        public event EventHandlers.CouldNotGetIpAddressHttpHeaderValuesEventHandler
            CouldNotGetIpAddressHttpHeaderValues;

        public event EventHandlers.CouldNotParseIpAddressHttpHeaderValuesEventHandler
            CouldNotParseIpAddressHttpHeaderValues;

        public event EventHandlers.IPAddressRangeCheckFailedEventHandler IPAddressRangeCheckFailed;

        public void Start(
            CloudServiceCredentials cloudServiceCredentials,
            ClientConfigSettings clientConfigSettings)
        {
            if (Initialised) return;
            EventMetaCache.Instance.EventMetaAdded += EventMetaAdded;
            EventMetaCache.Instance.AddEventMetaFailed += Instance_AddEventMetaFailed;
            EventMetaCache.Instance.AddEventMetaFailed += AddEventMetaFailed;
            EventMetaCache.Instance.GetEventMetadataPayloadBatchFailed += Instance_GetEventMetadataPayloadBatchFailed;
            EventMetaCache.Instance.GetEventMetadataPayloadBatchFailed += GetEventMetadataPayloadBatchFailed;
            EventMetaCache.Instance.GotEventMetadataPayloadBatch += GotEventMetadataPayloadBatch;
            EventMetaCache.Instance.ClearCacheFailed += Instance_ClearCacheFailed;
            EventMetaCache.Instance.ClearCacheFailed += ClearCacheFailed;
            EventTransmissionClient.Instance.InitialisationFailed += Instance_InitialisationFailed;
            EventTransmissionClient.Instance.InitialisationFailed += InitialisationFailed;
            EventTransmissionClient.Instance.TransmissionFailed += Instance_TransmissionFailed;
            EventTransmissionClient.Instance.TransmissionFailed += TransmissionFailed;
            EventTransmissionClient.Instance.DataTransmitted += Instance_DataTransmitted;
            EventTransmissionClient.Instance.DataTransmitted += DataTransmitted;

            JobScheduler.Instance.JobSchedulerStartFailed += Instance_JobSchedulerStartFailed;
            JobScheduler.Instance.JobSchedulerStartFailed += JobSchedulerStartFailed;
            JobScheduler.Instance.JobSchedulerShutdownFailed += Instance_JobSchedulerShutdownFailed;
            JobScheduler.Instance.JobSchedulerShutdownFailed += JobSchedulerShutdownFailed;
            JobScheduler.Instance.EventMetadataPublishJobExecutionFailed += Instance_EventMetadataPublishJobExecutionFailed;
            JobScheduler.Instance.EventMetadataPublishJobExecutionFailed += EventMetadataPublishJobExecutionFailed;
            JobScheduler.Instance.GetBlacklistJobExecutionFailed += Instance_GetBlacklistJobExecutionFailed;
            JobScheduler.Instance.GetBlacklistJobExecutionFailed += GetBlacklistJobExecutionFailed;
            JobScheduler.Instance.GetWhitelistJobExecutionFailed += GetWhitelistJobExecutionFailed;

            Blacklist.Instance.IpAddressAdded += BlacklistIpAddressAdded;
            Blacklist.Instance.AddIpAddressFailed += Instance_AddIpAddressFailed;
            Blacklist.Instance.AddIpAddressFailed += BlacklistAddIpAddressFailed;
            Blacklist.Instance.BlacklistUpdated += BlacklistListUpdated;
            Blacklist.Instance.GotLatestBlacklist += Instance_GotLatestBlacklist;
            Blacklist.Instance.GotLatestBlacklist += BlacklistGotLatestList;
            Blacklist.Instance.GotLatestBlacklistRanges += Instance_GotLatestBlacklistRanges;
            Blacklist.Instance.GotLatestBlacklistRanges += GotLatestBlacklistRanges;
            Blacklist.Instance.GetLatestBlacklistFailed += Instance_GetLatestBlacklistFailed;
            Blacklist.Instance.GetLatestBlacklistFailed += BlacklistGetLatestListFailed;
            Blacklist.Instance.CouldNotParseIpAddress += BlacklistCouldNotParseIpAddress;
            Blacklist.Instance.IPAddressRangeCheckFailed += Instance_IPAddressRangeCheckFailed;
            Blacklist.Instance.IPAddressRangeCheckFailed += IPAddressRangeCheckFailed;

            Whitelist.Instance.IpAddressAdded += WhitelistIpAddressAdded;
            Whitelist.Instance.AddIpAddressFailed += WhitelistAddIpAddressFailed;
            Whitelist.Instance.WhitelistUpdated += WhitelistListUpdated;
            Whitelist.Instance.GotLatestWhitelist += Instance_GotLatestWhitelist;
            Whitelist.Instance.GotLatestWhitelist += WhitelistGotLatestList;
            Whitelist.Instance.GetLatestWhitelistFailed += Instance_GetLatestWhitelistFailed;
            Whitelist.Instance.GotLatestWhitelistRanges += Instance_GotLatestWhitelistRanges;
            Whitelist.Instance.GotLatestWhitelistRanges += WhitelistGotLatestListRanges;
            Whitelist.Instance.GetLatestWhitelistFailed += WhitelistGetLatestListFailed;
            Whitelist.Instance.CouldNotParseIpAddress += WhitelistCouldNotParseIpAddress;
            ClientConfigSettings = clientConfigSettings;

            EventMetaCache.Instance.Initialised = true;
            EventTransmissionClient.Instance.InitAsync(
                cloudServiceCredentials,
                clientConfigSettings
            ).Wait();

            Blacklist.Instance.Init(clientConfigSettings);
            Whitelist.Instance.Init(clientConfigSettings);

            JobScheduler.Instance.StartAsync(
                EventTransmissionClient.Instance,
                EventMetaCache.Instance,
                Blacklist.Instance,
                Whitelist.Instance,
                clientConfigSettings).Wait();

            _initialised = true;
        }

        private void Instance_GotLatestWhitelistRanges(object sender, GotLatestListEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("infoMessage",
                    string.Concat("Whitelist ranges downloaded. No. items: ", e.NumIpAddresses));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicInfoEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_GotLatestBlacklistRanges(object sender, GotLatestListEventArgs e)
        {
#if NET461
            try
            {
                foreach (var attributes in e.RegionCounts.Select(regionCount => new Dictionary<string, object>
                    {{"region", regionCount.Key}, {"count", (float) regionCount.Value}}))
                    RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicBlacklistInfoEventName, attributes);
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_IPAddressRangeCheckFailed(object sender, IPAddressRangeCheckFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("IP address range-check failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_GetLatestWhitelistFailed(object sender, GetLatestListFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Get latest whitelist failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_GotLatestWhitelist(object sender, GotLatestListEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("infoMessage",
                    string.Concat("Whitelist downloaded. No. items: ", e.NumIpAddresses));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicInfoEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_GotLatestBlacklist(object sender, GotLatestListEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("infoMessage",
                    string.Concat("Blacklist downloaded. No. items: ", e.NumIpAddresses));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicInfoEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_DataTransmitted(object sender, EventTransmittedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("infoMessage",
                    string.Concat("Event meta transmitted. No. items: ", e.NumItemsTransmitted));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicBlacklistInfoEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_GetLatestBlacklistFailed(object sender, GetLatestListFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Get latest blacklist failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_AddIpAddressFailed(object sender, AddIpAddressFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Add IP address failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_GetBlacklistJobExecutionFailed(object sender, GetBlacklistJobExecutionFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Get blacklist job failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_EventMetadataPublishJobExecutionFailed(object sender, EventMetadataPublishJobExecutionFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Event meta publish job failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_JobSchedulerShutdownFailed(object sender, JobSchedulerShutdownFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Job-scheduler shutdown failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_JobSchedulerStartFailed(object sender, JobSchedulerStartFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Job-scheduler start failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_TransmissionFailed(object sender, EventTransmissionFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Event transmission failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_InitialisationFailed(object sender,
            EventTransmissionClientInitialisationFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Event transmission client initialisation failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_ClearCacheFailed(object sender, ClearCacheFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Clear cache failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_GetEventMetadataPayloadBatchFailed(object sender,
            GetEventMetadataPayloadBatchFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Get event meta payload failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        private void Instance_AddEventMetaFailed(object sender, AddEventMetaFailedEventArgs e)
        {
#if NET461
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Add event meta failed: ", e.Exception.Message));
                RecordNewRelicCustomEvent(ClientConfigSettings.NewRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        public void Shutdown()
        {
            if (!Initialised) return;
            try
            {
                JobScheduler.Instance.Shutdown();
                EventMetaCache.Instance.Initialised = false;
                EventMetaCache.Instance.Clear();
                Initialised = false;
            }
            catch (Exception)
            {
                // Ignore - exceptions are already handled in each call above
            }
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
                jsonObject.Add(!string.IsNullOrEmpty(queryString)
                    ? new JProperty("queryString", queryString)
                    : new JProperty("queryString", null));
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
        // todo: [LP] Add error handler
        public static IEnumerable<string> MaskIPAddresses(IEnumerable<string> ipAddresses)
        {
            if (ipAddresses == null) throw new ArgumentNullException(nameof(ipAddresses));
            var maskedIpAddresses = new List<string>();

            foreach (var ipAddress in ipAddresses)
            {
                if (!IPAddress.TryParse(ipAddress, out var ip)) continue;
                switch (ip.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                    {
                        var octets = ipAddress.Split('.');
                        octets[1] = "0";
                        octets[2] = "0";
                        maskedIpAddresses.Add(string.Join(".", octets));
                        break;
                    }
                    case AddressFamily.InterNetworkV6:
                    {
                        var hexes = ipAddress.Split(':');
                        hexes[1] = "0000";
                        hexes[2] = "0000";
                        maskedIpAddresses.Add(string.Join(":", hexes));
                        break;
                    }
                }
            }

            return maskedIpAddresses;
        }

        public static Dictionary<string, int> GroupByRegion(IEnumerable<IpAddressRangeMeta> ipAddressRangeMeta)
        {
            var regionGrouping = ipAddressRangeMeta.GroupBy(meta => meta.Region);
            return regionGrouping.ToDictionary(r => r.Key, r => r.Count());
        }

#if NET461
        public static bool TryParseIpAddresses(
            IEnumerable<string> ipAddressHttpHeaderValues,
            out HashSet<string> ipAddresses)
        {
            if (ipAddressHttpHeaderValues == null) throw new ArgumentNullException(nameof(ipAddressHttpHeaderValues));
            ipAddresses = new HashSet<string>();

            try
            {
                foreach (var ipAddressHttpHeaderValue in ipAddressHttpHeaderValues)
                {
                    var rawIpAddresses = ipAddressHttpHeaderValue.Split(',').Select(ip => ip.Trim());
                    foreach (var rawIpAddress in rawIpAddresses)
                    {
                        var isIpAddress = IPAddress.TryParse(rawIpAddress, out _);
                        if (isIpAddress)
                            ipAddresses.Add(rawIpAddress);
                        else
                            throw new Exception(rawIpAddress + " is not a valid IP address.");
                    }
                }

                return ipAddresses.Count > 0;
            }
            catch (Exception e)
            {
                throw new Exception("Could not parse IP addresses.", e);
            }
        }

        // todo: RYR need to subscribe to thrown error events here
        public void RecordNewRelicCustomEvent(
            string eventType,
            IEnumerable<KeyValuePair<string, object>> attributes)
        {
            if (string.IsNullOrEmpty(eventType)) throw new ArgumentNullException(nameof(eventType));
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));
            try
            {
                NewRelic.Api.Agent.NewRelic.RecordCustomEvent(eventType, attributes);
            }
            catch (Exception e)
            {
                OnRecordNewRelicCustomEventFailed(new RecordNewRelicCustomEventFailedEventArgs(eventType, e));
            }
        }
#endif

        public static bool UriEndpointShouldBeMonitored(string uri, string[] endpointsToMonitor)
        {
            if (string.IsNullOrEmpty(uri)) throw new ArgumentNullException(nameof(uri));
            if (endpointsToMonitor == null) throw new ArgumentNullException(nameof(endpointsToMonitor));

            bool endpointFound;
            var counter = 0;

            do
            {
                endpointFound = uri.Contains(endpointsToMonitor[counter++]);
            } while (!endpointFound && counter < endpointsToMonitor.Length - 1);

            return endpointFound;
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

        public void OnCouldNotGetIpAddressHttpHeaderValues(CouldNotGetIpAddressHttpHeaderValuesEventArgs e)
        {
            CouldNotGetIpAddressHttpHeaderValues?.Invoke(this, e);
        }

        public void OnCouldNotParseIpAddressHttpHeaderValues(CouldNotParseIpAddressHttpHeaderValuesEventArgs e)
        {
            CouldNotParseIpAddressHttpHeaderValues?.Invoke(this, e);
        }

        private void OnRecordNewRelicCustomEventFailed(RecordNewRelicCustomEventFailedEventArgs e)
        {
            RecordNewRelicCustomEventFailed?.Invoke(this, e);
        }

        private void OnGotLatestBlacklistRanges(GotLatestListEventArgs e)
        {
            GotLatestBlacklistRanges?.Invoke(this, e);
        }
    }
}