using System.Web.Http;
using Bwrx.Api;
using Newtonsoft.Json;

namespace WebApplication1
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    "DefaultApi",
            //    "api/{controller}/{id}",
            //    new {id = RouteParameter.Optional}
            //);

            var clientConfigSettings =
                JsonConvert.DeserializeObject<ClientConfigSettings>(Resources.ClientConfigSettings);

            var blockingDelegatingHandler = new BlockingDelegatingHandler(clientConfigSettings, GlobalConfiguration.Configuration);

            blockingDelegatingHandler.CouldNotGetIpAddressHttpHeaderValues +=
                BwrxDelegatingHandler_CouldNotGetIpAddressHttpHeaderValues;
            blockingDelegatingHandler.CouldNotParseIpAddressHttpHeaderValues +=
                BwrxDelegatingHandler_CouldNotParseIpAddressHttpHeaderValues;
            blockingDelegatingHandler.BlacklistedIpAddressDetected +=
                BwrxDelegatingHandler_BlacklistedIpAddressDetected;

            //config.MessageHandlers.Add(blockingDelegatingHandler);

            config.Routes.MapHttpRoute(
                name: "values",
                routeTemplate: "api/values",
                defaults: new { controller = "Values" },
                constraints: null,
                handler: blockingDelegatingHandler  // per-route message handler
            );
        }

        private static void BwrxDelegatingHandler_BlacklistedIpAddressDetected(object sender,
            BlacklistedIpAddressDetectedEventArgs e)
        {
        }

        private static void BwrxDelegatingHandler_CouldNotGetIpAddressHttpHeaderValues(object sender,
            CouldNotGetIpAddressHttpHeaderValuesEventArgs e)
        {
        }

        private static void BwrxDelegatingHandler_CouldNotParseIpAddressHttpHeaderValues(object sender,
            CouldNotParseIpAddressHttpHeaderValuesEventArgs e)
        {
        }
    }
}