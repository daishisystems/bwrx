using System.Collections.Generic;

namespace Bwrx.Api
{
    internal class IpAddressEntryAttempt
    {
        public IEnumerable<string> IpAddresses { get; set; }
        public bool PassiveMode { get; set; }
        public string Uri { get; set; }
        public string HttpMethod { get; set; }
        public string QueryString { get; set; }
        public string HttpContent { get; set; }
    }
}