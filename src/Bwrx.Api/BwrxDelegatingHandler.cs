using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bwrx.Api
{
#if NET461
    public class BwrxDelegatingHandler : DelegatingHandler // todo: log failures
    {
        private readonly HttpStatusCode _blockingHttpStatusCode;
        private readonly string _ipAddressHeaderName;
        private readonly bool _passiveMode;

        public BwrxDelegatingHandler(
            string ipAddressHeaderName,
            HttpStatusCode blockingHttpStatusCode,
            bool passiveMode = false)
        {
            if (string.IsNullOrEmpty(ipAddressHeaderName)) throw new ArgumentNullException(nameof(ipAddressHeaderName));

            _ipAddressHeaderName = ipAddressHeaderName;
            _blockingHttpStatusCode = blockingHttpStatusCode;
            _passiveMode = passiveMode;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var gotIpAddressHttpHeaders = Agent.TryGetIpAddressHttpHeaderValues(
                _ipAddressHeaderName,
                request,
                out var ipAddressHttpHeaderValues);

            if (!gotIpAddressHttpHeaders) return base.SendAsync(request, cancellationToken);

            var canParseIpAddressHeaders =
                Agent.TryParseIpAddresses(ipAddressHttpHeaderValues, out var ipAddresses);

            if (!canParseIpAddressHeaders) return base.SendAsync(request, cancellationToken);

            var blacklistedIpAddresses = ipAddresses
                .Where(ipAddress => Blacklist.Instance.IsIpAddressBlacklisted(ipAddress))
                .ToList();

            if (blacklistedIpAddresses.Count <= 0 || _passiveMode)
                return base.SendAsync(request, cancellationToken);

            var response = new HttpResponseMessage(_blockingHttpStatusCode);
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }
#endif
}