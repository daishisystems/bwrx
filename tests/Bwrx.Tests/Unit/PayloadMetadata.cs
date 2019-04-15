using System.Collections.Generic;

namespace Bwrx.Tests.Unit
{
    internal class PayloadMetadata
    {
        public string EventName { get; set; }
        public string QueryString { get; set; }
        public Dictionary<string, string> HttpHeaders { get; set; }
        public string Created { get; set; }
    }
}