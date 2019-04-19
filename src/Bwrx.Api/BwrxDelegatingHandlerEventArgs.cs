using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Bwrx.Api
{
#if NET461
    public class BwrxDelegatingHandlerEventArgs : EventArgs
    {
        public BwrxDelegatingHandlerEventArgs(
            HttpRequestMessage httpRequestMessage,
            IEnumerable<IPAddress> ipAddresses)
        {
            HttpRequestMessage = httpRequestMessage;
            IPAddresses = ipAddresses;
        }

        public HttpRequestMessage HttpRequestMessage { get; set; }
        public IEnumerable<IPAddress> IPAddresses { get; set; }
    }
#endif
}