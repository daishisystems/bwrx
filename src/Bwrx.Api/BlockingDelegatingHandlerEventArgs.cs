using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Bwrx.Api
{
#if NET461
    public class BlockingDelegatingHandlerEventArgs : EventArgs
    {
        public BlockingDelegatingHandlerEventArgs(
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