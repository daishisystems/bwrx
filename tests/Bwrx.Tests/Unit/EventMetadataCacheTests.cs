﻿using System;
using System.Collections.Generic;
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
            var eventMetadataCache = new EventMetaCache();
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
            var eventMetaCache = new EventMetaCache {MaxQueueLength = 10};

            dynamic payload = new ExpandoObject();
            payload.Name = "";

            var httpHeaders = new Dictionary<string, string>
                {{"User-Agent", "USERAGENT"}, {"Content-Type", "CONTENT"}};

            for (var i = 0; i < 11; i++)
                eventMetaCache.Add(
                    payload,
                    "BRANDCODE",
                    "EVENTNAME",
                    httpHeaders);

            Assert.Equal(10, eventMetaCache.NumItems);
        }

        [Fact]
        public void CantDequeueMoreThan1000EventsPerRun()
        {
            var eventMetadataCache = new EventMetaCache();
            Assert.Throws<IndexOutOfRangeException>(() => eventMetadataCache.GetEventMetadataPayloadBatch(1001));
        }

        [Fact]
        public void Top50EventMetadataPayloadsAreDequeued()
        {
            var eventMetadataCache = new EventMetaCache();
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

            var eventMetadataPayloadBatch = eventMetadataCache.GetEventMetadataPayloadBatch(50);
            Assert.Equal(50, eventMetadataPayloadBatch.Count());
        }
    }
}