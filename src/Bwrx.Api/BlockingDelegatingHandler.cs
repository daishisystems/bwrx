
#if NET461
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;

#endif
namespace Bwrx.Api
{
#if NET461
    public class BlockingDelegatingHandler : DelegatingHandler
    {
        private readonly int _blockingHttpStatusCode;
        private readonly string _ipAddressHeaderName;
        private readonly bool _passiveMode;
        private readonly string _newRelicInfoEventName;
        private readonly string _newRelicErrorEventName;

        public BlockingDelegatingHandler(ClientConfigSettings clientConfigSettings, HttpConfiguration httpConfiguration)
        {
            if (string.IsNullOrEmpty(clientConfigSettings.IpAddressHeaderName))
                throw new ArgumentNullException(nameof(clientConfigSettings.IpAddressHeaderName));
            InnerHandler = new HttpControllerDispatcher(httpConfiguration);
            _ipAddressHeaderName = clientConfigSettings.IpAddressHeaderName;
            _blockingHttpStatusCode = clientConfigSettings.BlockingHttpStatusCode;
            _passiveMode = clientConfigSettings.PassiveBlockingMode;
            _newRelicErrorEventName = clientConfigSettings.NewRelicErrorEventName;
            _newRelicInfoEventName = clientConfigSettings.NewRelicInfoEventName;
            BlacklistedIpAddressDetected += BlockingDelegatingHandler_BlacklistedIpAddressDetected;
            CouldNotParseIpAddressHttpHeaderValues += BlockingDelegatingHandler_CouldNotParseIpAddressHttpHeaderValues;
            CouldNotGetIpAddressHttpHeaderValues += BlockingDelegatingHandler_CouldNotGetIpAddressHttpHeaderValues;
            Agent.Instance.HandlerIsInitialised = true;
        }

        private void BlockingDelegatingHandler_CouldNotGetIpAddressHttpHeaderValues(
            object sender,
            CouldNotGetIpAddressHttpHeaderValuesEventArgs e)
        {
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Could not get IP address HTTP header values: ", e.Exception.Message));
                Agent.Instance.RecordNewRelicCustomEvent(_newRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void BlockingDelegatingHandler_CouldNotParseIpAddressHttpHeaderValues(
            object sender,
            CouldNotParseIpAddressHttpHeaderValuesEventArgs e)
        {
            try
            {
                var attributes = new KeyValuePair<string, object>("errorMessage",
                    string.Concat("Could not parse IP address HTTP headers: ", e.Exception.Message));
                Agent.Instance.RecordNewRelicCustomEvent(_newRelicErrorEventName, new[] {attributes});
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public event EventHandlers.CouldNotParseIpAddressHttpHeaderValuesEventHandler
            CouldNotParseIpAddressHttpHeaderValues;

        public event EventHandlers.CouldNotGetIpAddressHttpHeaderValuesEventHandler
            CouldNotGetIpAddressHttpHeaderValues;

        public event EventHandlers.BlacklistedIpAddressDetectedEventHandler BlacklistedIpAddressDetected;

        private void BlockingDelegatingHandler_BlacklistedIpAddressDetected(object sender,
            BlacklistedIpAddressDetectedEventArgs e)
        {
            try
            {
                foreach (var ipAddress in e.IPAddresses)
                {
                    var attributes = new KeyValuePair<string, object>("infoMessage",
                        string.Concat("Blacklisted IP address detected: ", ipAddress));
                    Agent.Instance.RecordNewRelicCustomEvent(_newRelicInfoEventName, new[] {attributes});
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

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
                // todo: Return here
            }

            if (!gotIpAddressHttpHeaders)
            {
                OnCouldNotGetIpAddressHttpHeaderValues(
                    new CouldNotGetIpAddressHttpHeaderValuesEventArgs(_ipAddressHeaderName));
                return await base.SendAsync(request, cancellationToken);
            }

            var canParseIpAddressHeaders = false;
            HashSet<string> ipAddresses = null;
            try
            {
                canParseIpAddressHeaders = Agent.TryParseIpAddresses(ipAddressHttpHeaderValues, out ipAddresses);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not parse IP addresses from HTTP headers";
                OnCouldNotParseIpAddressHttpHeaderValues(new CouldNotParseIpAddressHttpHeaderValuesEventArgs(
                    request.Headers.GetValues(_ipAddressHeaderName), new Exception(errorMessage, exception)));
                // todo: Return here
            }

            if (!canParseIpAddressHeaders)
            {
                OnCouldNotParseIpAddressHttpHeaderValues(new CouldNotParseIpAddressHttpHeaderValuesEventArgs(
                    request.Headers.GetValues(_ipAddressHeaderName)));
                return await base.SendAsync(request, cancellationToken);
            }
            // todo: Can I whitelist CloudFront here? If so, will it catch associated bots?
            List<string> blacklistedIpAddresses;
            try
            {
                blacklistedIpAddresses = ipAddresses
                    .Where(ipAddress => Blacklist.Instance.IsIpAddressBlacklisted(ipAddress))
                    .ToList();
            }
            catch (Exception)
            {
                blacklistedIpAddresses = new List<string>();
                // todo: Raise custom error handler
            }

            if (blacklistedIpAddresses.Count == 0) return await base.SendAsync(request, cancellationToken);
            // todo: try-catch here
            var canParseBlockingHttpStatusCode =
                Enum.TryParse(_blockingHttpStatusCode.ToString(), out HttpStatusCode blockingHttpStatusCode);
            if (!canParseBlockingHttpStatusCode) blockingHttpStatusCode = HttpStatusCode.Forbidden;
            try
            {
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
                var maskedIPAddresses = Agent.MaskIPAddresses(blacklistedIpAddresses);
                OnBlacklistedIpAddressDetected(
                    new BlacklistedIpAddressDetectedEventArgs(
                        maskedIPAddresses,
                        _passiveMode,
                        request.RequestUri.ToString(),
                        request.Method.ToString(),
                        request.RequestUri.Query,
                        httpContent));
            }
            catch (Exception)
            {
                // todo: [LP] New event handler [Failed to process bot request]
            }

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