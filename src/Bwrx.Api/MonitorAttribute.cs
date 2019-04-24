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

namespace Bwrx.Api // todo: event handler, public Agent methods ...
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
            catch
            {
                return result;
            }

            if (!gotIpAddressHttpHeaders) return result;

            bool canParseIpAddressHeaders;
            IEnumerable<IPAddress> ipAddresses;
            try
            {
                canParseIpAddressHeaders = Agent.TryParseIpAddresses(ipAddressHttpHeaderValues, out ipAddresses);
            }
            catch
            {
                return result;
            }

            if (!canParseIpAddressHeaders) return result;

            try
            {
                var rawIpAddresses = ipAddresses.Select(ip => ip.ToString());
                var payload = actionContext.Request.GetQueryNameValuePairs();

                EventMetaCache.Instance.Add(EventName, payload, rawIpAddresses, actionContext.Request.Headers);
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