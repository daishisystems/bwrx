#if NET461
using System.Linq;
using System.Collections.Specialized;
#endif
using System.Collections.Generic;
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
            const string ipAddress1 = "83c881f4-03a3-192.168.0.14fd2-9622-9fa42427ddeb";
            const string ipAddress2 = "83c881f4-03a3-192.168.0.1192.168.0.24fd2-9622-9fa42427ddeb";
            const string ipAddress3 = "83c881f4-03a3-192.168.0.14fd2-9622-9fa42427ddeb";

            var ipAddressText = new List<string> {ipAddress1, ipAddress2, ipAddress3};
            var ipAddressesAreParsed = Agent.TryParseIpAddresses(ipAddressText, out var ipAddresses);

            Assert.True(ipAddressesAreParsed);
            Assert.Equal(4, ipAddresses.Count());
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
    }
}