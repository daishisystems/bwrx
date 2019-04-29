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
        }

        public event EventHandlers.CouldNotParseIpAddressHttpHeaderValuesEventHandler
            CouldNotParseIpAddressHttpHeaderValues;

        public event EventHandlers.CouldNotGetIpAddressHttpHeaderValuesEventHandler
            CouldNotGetIpAddressHttpHeaderValues;

        public event EventHandlers.BlacklistedIpAddressDetectedEventHandler BlacklistedIpAddressDetected;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
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
                return base.SendAsync(request, cancellationToken);
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
                return base.SendAsync(request, cancellationToken);
            }

            var blacklistedIpAddresses = ipAddresses
                .Where(ipAddress => Blacklist.Instance.IsIpAddressBlacklisted(ipAddress))
                .ToList();

            if (blacklistedIpAddresses.Count == 0) return base.SendAsync(request, cancellationToken);

            OnBlacklistedIpAddressDetected(
                new BlacklistedIpAddressDetectedEventArgs(blacklistedIpAddresses, _passiveMode));
            if (_passiveMode) return base.SendAsync(request, cancellationToken);

            var canParseBlockingHttpStatusCode =
                Enum.TryParse(_blockingHttpStatusCode.ToString(), out HttpStatusCode blockingHttpStatusCode);

            if (!canParseBlockingHttpStatusCode) blockingHttpStatusCode = HttpStatusCode.Forbidden;

            var response = new HttpResponseMessage(blockingHttpStatusCode);
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
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