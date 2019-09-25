
using System;
#if NET461
using System.Linq;
using System.Collections.Specialized;
#endif
using System.Collections.Generic;
using System.Linq;
using Bwrx.Api;
using Newtonsoft.Json;
using Xunit;

#if NETCOREAPP2_1
using Microsoft.AspNetCore.Http;

#endif

namespace Bwrx.Tests.Unit
{
    public class AgentTests
    {
#if NETCOREAPP2_1
        [Fact]
        public void HttpHeadersAreExtractedInKvFormat()
        {
            const string key1 = "KEY1";
            const string key2 = "KEY2";
            const string value1 = "VAL1";
            const string value2 = "VAL2";

            IHeaderDictionary httpHeadersDictionary = new HeaderDictionary
            {
                {key1, value1},
                {key2, value2}
            };

            var httpHeaders = Agent.ParseHttpHeaders(httpHeadersDictionary);

            Assert.True(httpHeaders.ContainsKey(key1));
            Assert.True(httpHeaders.ContainsKey(key2));
            Assert.True(httpHeaders.ContainsValue(value1));
            Assert.True(httpHeaders.ContainsValue(value2));
        }
#endif

#if NET461
        [Fact]
        public void HttpHeadersAreExtractedInKvFormat()
        {
            const string key1 = "KEY1";
            const string key2 = "KEY2";
            const string value1 = "VAL1";
            const string value2 = "VAL2";

            var httpHeadersCollection = new NameValueCollection {{key1, value1}, {key2, value2}};

            var httpHeaders = Agent.ParseHttpHeaders(httpHeadersCollection);

            Assert.True(httpHeaders.ContainsKey(key1));
            Assert.True(httpHeaders.ContainsKey(key2));
            Assert.True(httpHeaders.ContainsValue(value1));
            Assert.True(httpHeaders.ContainsValue(value2));
        }

        [Fact]
        public void IpAddressesAreParsed()
        {
            const string ipAddress1 = "192.168.0.14";
            const string ipAddress2 = "2a02:c7d:b9b:6700:2108:fdfd:b367:c460 ";

            var ipAddressText = new List<string> {ipAddress1, ipAddress2};
            var ipAddressesAreParsed = Agent.TryParseIpAddresses(ipAddressText, out var ipAddresses);

            Assert.True(ipAddressesAreParsed);
            Assert.Equal(2, ipAddresses.Count());
        }

        [Fact]
        public void UriEndpointShouldBeMonitored()
        {
            var endpointsToMonitor = new List<string>
            {
                "availability",
                "booking",
                "test"
            };

            const string uri = "https://example.com/v4/availability?ADT=1&CHD=0&DateIn=2019-05-23&D";
            var uriEndpointShouldBeMonitored = Agent.UriEndpointShouldBeMonitored(uri, endpointsToMonitor.ToArray());

            Assert.True(uriEndpointShouldBeMonitored);
        }

        [Fact]
        public void UriEndpointShouldNotBeMonitored()
        {
            var endpointsToMonitor = new List<string>
            {
                "search",
                "booking",
                "test"
            };

            const string uri = "https://example.com/v4/availability?ADT=1&CHD=0&DateIn=2019-05-23&D";
            var uriEndpointShouldBeMonitored = Agent.UriEndpointShouldBeMonitored(uri, endpointsToMonitor.ToArray());

            Assert.False(uriEndpointShouldBeMonitored);
        }

#endif

        [Fact]
        public void TrackingMetadataIsAddedToJSON()
        {
            const string eventName = "BOOTUP";
            const string queryString = "QUERY-STRING";
            var httpHeaders = new Dictionary<string, string>
                {{"User-Agent", "USERAGENT"}, {"Content-Type", "CONTENT"}};
            const string created = "1538645229";

            var updatedJSON = Agent.AddTrackingMetadataToJson(
                "{}",
                eventName,
                queryString,
                httpHeaders,
                created);

            var payloadMetadata = JsonConvert.DeserializeObject<PayloadMetadata>(updatedJSON);

            Assert.Equal("BOOTUP", payloadMetadata.EventName);
            Assert.Equal("QUERY-STRING", payloadMetadata.QueryString);
            Assert.Equal("USERAGENT", payloadMetadata.HttpHeaders["User-Agent"]);
            Assert.Equal("CONTENT", payloadMetadata.HttpHeaders["Content-Type"]);
            Assert.Equal("1538645229", payloadMetadata.Created);
        }

        [Fact]
        public void RegionsAreGroupedAndCounted()
        {
            var ipAddressRangeMeta = new List<IpAddressRangeMeta>
            {
                new IpAddressRangeMeta {IpAddressRange = "", Region = "India"},
                new IpAddressRangeMeta {IpAddressRange = "", Region = "Spain"},
                new IpAddressRangeMeta {IpAddressRange = "", Region = "Spain"}
            };

            var grouping = Agent.GroupByRegion(ipAddressRangeMeta);
            Assert.Equal(2, grouping["Spain"]);
            Assert.Equal(1, grouping["India"]);
        }

        [Fact]
        public void IPAddressesAreMasked()
        {
            var ipAddresses = new List<string> {"127.1.10.1", "2001:0db8:85a3:0000:0000:8a2e:0370:7334"};

            var maskedIPAddresses = Agent.MaskIPAddresses(ipAddresses).ToList();
            Assert.Equal("127.0.0.1", maskedIPAddresses[0]);
            Assert.Equal("2001:0000:0000:0000:0000:8a2e:0370:7334", maskedIPAddresses[1]);
        }
    }
}