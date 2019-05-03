using System.Collections.Generic;
#if NET461
using System.Net.Http.Headers;

#endif

namespace Bwrx.Api
{
#if NET461
    internal class QueryStringPayload
    {
        public IEnumerable<KeyValuePair<string, string>> QueryString { get; set; }
        public string EventName { get; set; }
        public HttpRequestHeaders HttpRequestHeaders { get; set; }
        public string Timestamp { get; set; }
        public IEnumerable<string> IPAddresses { get; set; }
    }
#endif
}