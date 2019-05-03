using System.Collections.Generic;

namespace Bwrx.Api
{
    internal class IpAddressEntryAttempt
    {
        public IEnumerable<string> IpAddresses { get; set; }
        public bool PassiveMode { get; set; }
    }
}