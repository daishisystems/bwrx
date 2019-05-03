using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
#if NET461
using System.Net.Http.Headers;

#endif

namespace Bwrx.Api
{
    public class EventMetaCache
    {
        private static readonly Lazy<EventMetaCache> Lazy =
            new Lazy<EventMetaCache>(() => new EventMetaCache());

        private ConcurrentQueue<string> _cache;

        public EventMetaCache()
        {
            _cache = new ConcurrentQueue<string>();
        }

        public long NumItems => _cache.Count;

        public static EventMetaCache Instance => Lazy.Value;

        public int MaxQueueLength { get; set; } = 175000;

        public event EventHandlers.EventMetaAddedEventHandler EventMetaAdded;
        public event EventHandlers.AddEventMetaFailedEventHandler AddEventMetaFailed;
        public event EventHandlers.GetEventMetadataPayloadBatchFailedEventHandler GetEventMetadataPayloadBatchFailed;
        public event EventHandlers.GetEventMetadataPayloadBatchEventHandler GotEventMetadataPayloadBatch;
        public event EventHandlers.ClearCacheFailedEventHandler ClearCacheFailed;

        public void Add<T>(
            T eventMetadataPayload,
            string eventName,
            string queryString = null,
            Dictionary<string, string> httpHeaders = null)
        {
            if (eventMetadataPayload == null)
                throw new ArgumentNullException(nameof(eventMetadataPayload));
            if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));            

            if (_cache == null)
            {
                _cache = new ConcurrentQueue<string>();
            }
            else if (_cache.Count >= MaxQueueLength)
            {
                const string errorMessage = "The cache is full.";
                OnAddEventMetaFailed(new AddEventMetaFailedEventArgs(new Exception(errorMessage)));
                return;
            }

            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None
                };

                var serialisedEventMetadataPayload = JsonConvert.SerializeObject(
                    eventMetadataPayload,
                    jsonSerializerSettings);

                var eventTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

                var cachePayload = Agent.AddTrackingMetadataToJson(
                    serialisedEventMetadataPayload,
                    eventName,
                    queryString,
                    httpHeaders,
                    eventTimestamp.ToString()
                );

                _cache.Enqueue(cachePayload);
                OnEventMetaAdded(new EventMetaAddedEventArgs(cachePayload));
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while adding event-meta to cache.";
                OnAddEventMetaFailed(new AddEventMetaFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }

#if NET461
        public void Add(
            string eventName,
            IEnumerable<KeyValuePair<string, string>> queryString,
            IEnumerable<string> ipAddresses,
            HttpRequestHeaders httpRequestHeaders)
        {
            if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));
            if (queryString == null) throw new ArgumentNullException(nameof(queryString));
            if (ipAddresses == null) throw new ArgumentNullException(nameof(ipAddresses));
            if (httpRequestHeaders == null) throw new ArgumentNullException(nameof(httpRequestHeaders));

            if (_cache == null)
            {
                _cache = new ConcurrentQueue<string>();
            }
            else if (_cache.Count >= MaxQueueLength)
            {
                const string errorMessage = "The cache is full.";
                OnAddEventMetaFailed(new AddEventMetaFailedEventArgs(new Exception(errorMessage)));
                return;
            }

            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None
                };

                var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                var flightAvailabilityPayload = new QueryStringPayload
                {
                    EventName = eventName,
                    QueryString = queryString,
                    HttpRequestHeaders = httpRequestHeaders,
                    IPAddresses = ipAddresses,
                    Timestamp = timestamp.ToString()
                };

                var eventMetadataPayload =
                    JsonConvert.SerializeObject(flightAvailabilityPayload, jsonSerializerSettings);

                _cache.Enqueue(eventMetadataPayload);
                OnEventMetaAdded(new EventMetaAddedEventArgs(eventMetadataPayload));
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while adding event-meta to cache.";
                OnAddEventMetaFailed(new AddEventMetaFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }
#endif

        public IEnumerable<string> GetEventMetadataPayloadBatch(
            int maxItemsToRemove = 1000)
        {
            if (maxItemsToRemove > 1000)
                throw new IndexOutOfRangeException(
                    $"Value {maxItemsToRemove} should be in range [1, 1000].");
            if (_cache == null || _cache.IsEmpty) return new List<string>();

            var eventMetadataPayloadBatch = new List<string>();
            var counter = 0;

            try
            {
                bool canDequeue;
                do
                {
                    canDequeue = _cache.TryDequeue(out var eventMetadataPayload);
                    if (canDequeue) eventMetadataPayloadBatch.Add(eventMetadataPayload);
                    counter++;
                } while (counter < maxItemsToRemove && canDequeue);

                OnGotEventMetadataPayloadBatch(new GetEventMetadataPayloadBatchEventArgs(
                    eventMetadataPayloadBatch.Count,
                    _cache.Count));
                return eventMetadataPayloadBatch;
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while getting the event-meta payload batch.";
                OnGetEventMetadataPayloadBatchFailed(new GetEventMetadataPayloadBatchFailedEventArgs(
                    new Exception(errorMessage, exception), _cache.Count));
            }

            return null;
        }

        public void Clear()
        {
            if (_cache == null) return;

            try
            {
                bool canDequeue;
                do
                {
                    canDequeue = _cache.TryDequeue(out _);
                } while (canDequeue);
            }
            catch (Exception exception)
            {
                const string errorMessage = "An error occurred while clearing the cache.";
                OnClearCacheFailed(new ClearCacheFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }

        protected virtual void OnEventMetaAdded(EventMetaAddedEventArgs e)
        {
            EventMetaAdded?.Invoke(this, e);
        }

        protected virtual void OnAddEventMetaFailed(AddEventMetaFailedEventArgs e)
        {
            AddEventMetaFailed?.Invoke(this, e);
        }

        protected virtual void OnGetEventMetadataPayloadBatchFailed(GetEventMetadataPayloadBatchFailedEventArgs e)
        {
            GetEventMetadataPayloadBatchFailed?.Invoke(this, e);
        }

        protected virtual void OnGotEventMetadataPayloadBatch(GetEventMetadataPayloadBatchEventArgs e)
        {
            GotEventMetadataPayloadBatch?.Invoke(this, e);
        }

        protected virtual void OnClearCacheFailed(ClearCacheFailedEventArgs e)
        {
            ClearCacheFailed?.Invoke(this, e);
        }
    }
}