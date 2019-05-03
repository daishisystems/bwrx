using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if NET461
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

#endif

namespace Bwrx.Api
{
#if NET461
    public class MonitorAttribute : Attribute, IActionFilter
    {
        public MonitorAttribute(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; }
        public bool AllowMultiple { get; }

        public Task<HttpResponseMessage> ExecuteActionFilterAsync(
            HttpActionContext actionContext,
            CancellationToken cancellationToken,
            Func<Task<HttpResponseMessage>> continuation)
        {
            var result = continuation();
            result.Wait(cancellationToken);

            bool gotIpAddressHttpHeaders;
            IEnumerable<string> ipAddressHttpHeaderValues;
            try
            {
                gotIpAddressHttpHeaders = Agent.TryGetIpAddressHttpHeaderValues(
                    Agent.Instance.ClientConfigSettings.IpAddressHeaderName,
                    actionContext.Request,
                    out ipAddressHttpHeaderValues);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not get IP address HTTP headers.";
                Agent.Instance.OnCouldNotGetIpAddressHttpHeaderValues(
                    new CouldNotGetIpAddressHttpHeaderValuesEventArgs(
                        Agent.Instance.ClientConfigSettings.IpAddressHeaderName,
                        new Exception(errorMessage, exception)));
                return result;
            }

            if (!gotIpAddressHttpHeaders)
            {
                Agent.Instance.OnCouldNotGetIpAddressHttpHeaderValues(
                    new CouldNotGetIpAddressHttpHeaderValuesEventArgs(
                        Agent.Instance.ClientConfigSettings.IpAddressHeaderName));
                return result;
            }

            bool canParseIpAddressHeaders;
            IEnumerable<IPAddress> ipAddresses;
            try
            {
                canParseIpAddressHeaders = Agent.TryParseIpAddresses(ipAddressHttpHeaderValues, out ipAddresses);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Could not parse IP addresses from HTTP headers";
                Agent.Instance.OnCouldNotParseIpAddressHttpHeaderValues(
                    new CouldNotParseIpAddressHttpHeaderValuesEventArgs(
                        actionContext.Request.Headers.GetValues(Agent.Instance.ClientConfigSettings
                            .IpAddressHeaderName),
                        new Exception(errorMessage, exception)));
                return result;
            }

            if (!canParseIpAddressHeaders)
            {
                Agent.Instance.OnCouldNotParseIpAddressHttpHeaderValues(
                    new CouldNotParseIpAddressHttpHeaderValuesEventArgs(
                        actionContext.Request.Headers.GetValues(Agent.Instance.ClientConfigSettings
                            .IpAddressHeaderName)));
                return result;
            }

            try
            {
                var rawIpAddresses = ipAddresses.Select(ip => ip.ToString());
                var queryString = actionContext.Request.GetQueryNameValuePairs();

                EventMetaCache.Instance.Add(EventName, queryString, rawIpAddresses, actionContext.Request.Headers);
                return result;
            }
            catch
            {
                return result;
            }
        }
    }
#endif
}