using System;
using System.Collections.Generic;
using System.Net;

namespace Bwrx.Api
{
    public class BlacklistedIpAddressDetectedEventArgs : EventArgs
    {
        public BlacklistedIpAddressDetectedEventArgs(
            IEnumerable<IPAddress> ipAddresses,
            bool passiveMode,
            string uri,
            string httpMethod,
            string queryString,
            string httpContent)
        {
            IPAddresses = ipAddresses;
            PassiveMode = passiveMode;
            Uri = uri;
            HttpMethod = httpMethod;
            QueryString = queryString;
            HttpContent = httpContent;
        }

        public IEnumerable<IPAddress> IPAddresses { get; }
        public bool PassiveMode { get; }
        public string Uri { get; }
        public string HttpMethod { get; }
        public string QueryString { get; }
        public string HttpContent { get; }
    }
}