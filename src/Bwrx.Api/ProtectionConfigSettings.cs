using System.Collections.Generic;

namespace Bwrx.Api
{
    public class ProtectionConfigSettings
    {
        public IEnumerable<string> EndpointsToMonitor { get; set; }
        public int BlockingHttpStatusCode { get; set; }
        public string IpAddressHeaderName { get; set; }
        public bool PassiveBlockingMode { get; set; }
    }
}