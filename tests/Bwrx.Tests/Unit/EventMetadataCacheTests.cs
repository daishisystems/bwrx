﻿using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Unit
{
    public class EventMetadataCacheTests
    {
        [Fact]
        public void AllEventMetadataPayloadsAreDequeued()
        {
            var eventMetadataCache = new EventMetaCache {Initialised = true};
            var httpHeaders = new Dictionary<string, string>
                {{"User-Agent", "USERAGENT"}, {"Content-Type", "CONTENT"}};

            for (var i = 0; i < 100; i++)
            {
                dynamic eventMetadata = new ExpandoObject();
                eventMetadata.Name = "EVENT";

                eventMetadataCache.Add(
                    eventMetadata,
                    "TEST",
                    "QUERY",
                    httpHeaders);
            }

            var eventMetadataPayloadBatch = eventMetadataCache.GetEventMetadataPayloadBatch();
            Assert.Equal(100, eventMetadataPayloadBatch.Count());
        }

        [Fact]
        public void CacheSizeLimitIsImposed()
        {
            var eventMetadataCache = new EventMetaCache {Initialised = true, MaxQueueLength = 10};
            dynamic payload = new ExpandoObject();
            payload.Name = "";

            var httpHeaders = new Dictionary<string, string>
                {{"User-Agent", "USERAGENT"}, {"Content-Type", "CONTENT"}};

            for (var i = 0; i < 11; i++)
                eventMetadataCache.Add(
                    payload,
                    "BRANDCODE",
                    "EVENTNAME",
                    httpHeaders);

            Assert.Equal(10, eventMetadataCache.NumItems);
        }

        [Fact]
        public void Top50EventMetadataPayloadsAreDequeued()
        {
            var eventMetadataCache = new EventMetaCache {MaxItemsToDequeue = 50, Initialised = true};

            var httpHeaders = new Dictionary<string, string>
                {{"User-Agent", "USERAGENT"}, {"Content-Type", "CONTENT"}};

            dynamic eventMetadata = new ExpandoObject();
            eventMetadata.Name = "";

            for (var i = 0; i < 100; i++)
                eventMetadataCache.Add(
                    eventMetadata,
                    "TEST",
                    "QUERY",
                    httpHeaders);

            var eventMetadataPayloadBatch = eventMetadataCache.GetEventMetadataPayloadBatch();
            Assert.Equal(50, eventMetadataPayloadBatch.Count());
        }
    }
}