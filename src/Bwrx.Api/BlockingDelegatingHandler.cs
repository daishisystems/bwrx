#if NET461
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#endif

namespace Bwrx.Api
{
#if NET461
    public class BlockingDelegatingHandler : DelegatingHandler
    {
        private readonly int _blockingHttpStatusCode;
        private readonly string _ipAddressHeaderName;
        private readonly bool _passiveMode;

        public BlockingDelegatingHandler(ClientConfigSettings clientConfigSettings)
        {
            if (string.IsNullOrEmpty(clientConfigSettings.IpAddressHeaderName))
                throw new ArgumentNullException(nameof(clientConfigSettings.IpAddressHeaderName));
            _ipAddressHeaderName = clientConfigSettings.IpAddressHeaderName;
            _blockingHttpStatusCode = clientConfigSettings.BlockingHttpStatusCode;
            _passiveMode = clientConfigSettings.PassiveBlockingMode;
            Agent.Instance.HandlerIsInitialised = true;
        }

        public event EventHandlers.CouldNotParseIpAddressHttpHeaderValuesEventHandler
            CouldNotParseIpAddressHttpHeaderValues;

        public event EventHandlers.CouldNotGetIpAddressHttpHeaderValuesEventHandler
            CouldNotGetIpAddressHttpHeaderValues;

        public event EventHandlers.BlacklistedIpAddressDetectedEventHandler BlacklistedIpAddressDetected;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!Agent.Instance.Initialised) return await base.SendAsync(request, cancellationToken);

            var gotIpAddressHttpHeaders = false;
            IEnumerable<string> ipAddressHttpHeaderValues = null;
            try
            {
                gotIpAddressHttpHeaders = Agent.TryGetIpAddressHttpHeaderValues(
                    _ipAddressHeaderName,
                    request,
                    out ipAddressHttpHeaderValues);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not get IP address HTTP headers.";
                OnCouldNotGetIpAddressHttpHeaderValues(new CouldNotGetIpAddressHttpHeaderValuesEventArgs(
                    _ipAddressHeaderName, new Exception(errorMessage, exception)));
            }

            if (!gotIpAddressHttpHeaders)
            {
                OnCouldNotGetIpAddressHttpHeaderValues(
                    new CouldNotGetIpAddressHttpHeaderValuesEventArgs(_ipAddressHeaderName));
                return await base.SendAsync(request, cancellationToken);
            }

            var canParseIpAddressHeaders = false;
            IEnumerable<IPAddress> ipAddresses = null;
            try
            {
                canParseIpAddressHeaders = Agent.TryParseIpAddresses(ipAddressHttpHeaderValues, out ipAddresses);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not parse IP addresses from HTTP headers";
                OnCouldNotParseIpAddressHttpHeaderValues(new CouldNotParseIpAddressHttpHeaderValuesEventArgs(
                    request.Headers.GetValues(_ipAddressHeaderName), new Exception(errorMessage, exception)));
            }

            if (!canParseIpAddressHeaders)
            {
                OnCouldNotParseIpAddressHttpHeaderValues(new CouldNotParseIpAddressHttpHeaderValuesEventArgs(
                    request.Headers.GetValues(_ipAddressHeaderName)));
                return await base.SendAsync(request, cancellationToken);
            }

            // todo: All IPs are flagged as malicious, including blacklisted IP.
            // Could cause issues if an IP originates from AWS, RYR ...
            var blacklistedIpAddresses = ipAddresses
                .Where(ipAddress => Blacklist.Instance.IsIpAddressBlacklisted(ipAddress))
                .ToList();

            if (blacklistedIpAddresses.Count == 0) return await base.SendAsync(request, cancellationToken);
            
            var canParseBlockingHttpStatusCode =
                Enum.TryParse(_blockingHttpStatusCode.ToString(), out HttpStatusCode blockingHttpStatusCode);

            if (!canParseBlockingHttpStatusCode) blockingHttpStatusCode = HttpStatusCode.Forbidden;
            var httpContent = await request.Content.ReadAsStringAsync();

            var ipAddressEntryAttempt = new IpAddressEntryAttempt
            {
                IpAddresses = blacklistedIpAddresses.Select(ip => ip.ToString()),
                PassiveMode = _passiveMode,
                Uri = request.RequestUri.ToString(),
                HttpMethod = request.Method.ToString(),
                QueryString = request.RequestUri.Query,
                HttpContent = httpContent
            };
            EventMetaCache.Instance.Add(ipAddressEntryAttempt, "Scrape");

            OnBlacklistedIpAddressDetected(
                new BlacklistedIpAddressDetectedEventArgs(
                    blacklistedIpAddresses,
                    _passiveMode,
                    request.RequestUri.ToString(),
                    request.Method.ToString(),
                    request.RequestUri.Query,
                    httpContent));

            if (_passiveMode) return await base.SendAsync(request, cancellationToken);

            var response = new HttpResponseMessage(blockingHttpStatusCode);
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return await tsc.Task;
        }

        private void OnCouldNotParseIpAddressHttpHeaderValues(
            CouldNotParseIpAddressHttpHeaderValuesEventArgs e)
        {
            CouldNotParseIpAddressHttpHeaderValues?.Invoke(this, e);
        }

        private void OnCouldNotGetIpAddressHttpHeaderValues(CouldNotGetIpAddressHttpHeaderValuesEventArgs e)
        {
            CouldNotGetIpAddressHttpHeaderValues?.Invoke(this, e);
        }

        private void OnBlacklistedIpAddressDetected(BlacklistedIpAddressDetectedEventArgs e)
        {
            BlacklistedIpAddressDetected?.Invoke(this, e);
        }
    }
#endif
}