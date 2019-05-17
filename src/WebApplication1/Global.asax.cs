using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Bwrx.Api;
using Newtonsoft.Json;

namespace WebApplication1
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var credentials = JsonConvert.DeserializeObject<CloudServiceCredentials>(Resources.Credentials);
            var config = JsonConvert.DeserializeObject<ClientConfigSettings>(Resources.ClientConfigSettings);

            Agent.Instance.AddEventMetaFailed += Instance_AddEventMetaFailed;
            Agent.Instance.BlacklistAddIpAddressFailed += Instance_BlacklistAddIpAddressFailed;
            Agent.Instance.BlacklistCouldNotParseIpAddress += Instance_BlacklistCouldNotParseIpAddress;
            Agent.Instance.BlacklistGetLatestListFailed += Instance_BlacklistGetLatestListFailed;
            Agent.Instance.BlacklistGotLatestList += Instance_BlacklistGotLatestList;
            Agent.Instance.BlacklistIpAddressAdded += Instance_BlacklistIpAddressAdded;
            Agent.Instance.BlacklistListUpdated += Instance_BlacklistListUpdated;
            Agent.Instance.ClearCacheFailed += Instance_ClearCacheFailed;
            Agent.Instance.CloudDatabaseConnectionFailed += Instance_CloudDatabaseConnectionFailed;
            Agent.Instance.CouldNotGetIpAddressHttpHeaderValues += Instance_CouldNotGetIpAddressHttpHeaderValues;
            Agent.Instance.CouldNotParseIpAddressHttpHeaderValues += Instance_CouldNotParseIpAddressHttpHeaderValues;
            Agent.Instance.DataTransmitted += Instance_DataTransmitted;
            Agent.Instance.EventMetadataPublishJobExecutionFailed += Instance_EventMetadataPublishJobExecutionFailed;
            Agent.Instance.EventMetaAdded += Instance_EventMetaAdded;
            Agent.Instance.GetWhitelistJobExecutionFailed += Instance_GetWhitelistJobExecutionFailed;
            Agent.Instance.GetBlacklistJobExecutionFailed += Instance_GetBlacklistJobExecutionFailed;
            Agent.Instance.GetEventMetadataPayloadBatchFailed += Instance_GetEventMetadataPayloadBatchFailed;
            Agent.Instance.GotEventMetadataPayloadBatch += Instance_GotEventMetadataPayloadBatch;
            Agent.Instance.JobSchedulerStartFailed += Instance_JobSchedulerStartFailed;
            Agent.Instance.InitialisationFailed += Instance_InitialisationFailed;
            Agent.Instance.TransmissionFailed += Instance_TransmissionFailed;
            Agent.Instance.WhitelistAddIpAddressFailed += Instance_WhitelistAddIpAddressFailed;
            Agent.Instance.WhitelistCouldNotParseIpAddress += Instance_WhitelistCouldNotParseIpAddress;
            Agent.Instance.WhitelistGetLatestListFailed += Instance_WhitelistGetLatestListFailed;
            Agent.Instance.WhitelistGotLatestList += Instance_WhitelistGotLatestList;
            Agent.Instance.WhitelistIpAddressAdded += Instance_WhitelistIpAddressAdded;
            Agent.Instance.WhitelistListUpdated += Instance_WhitelistListUpdated;

            Agent.Instance.Start(credentials, config);
        }

        private void Instance_WhitelistListUpdated(object sender, EventArgs e)
        {
            // ignore
        }

        private void Instance_WhitelistIpAddressAdded(object sender, IpAddressAddedEventArgs e)
        {
            // ignore
        }

        private void Instance_WhitelistGotLatestList(object sender, GotLatestListEventArgs e)
        {
            // ignore
        }

        private void Instance_WhitelistGetLatestListFailed(object sender, GetLatestListFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_WhitelistCouldNotParseIpAddress(object sender, CouldNotParseIpAddressEventArgs e)
        {
            // ignore
        }

        private void Instance_WhitelistAddIpAddressFailed(object sender, AddIpAddressFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_TransmissionFailed(object sender, EventTransmissionFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_InitialisationFailed(object sender,
            EventTransmissionClientInitialisationFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_JobSchedulerStartFailed(object sender, JobSchedulerStartFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_GotEventMetadataPayloadBatch(object sender, GetEventMetadataPayloadBatchEventArgs e)
        {
            // ignore
        }

        private void Instance_GetEventMetadataPayloadBatchFailed(object sender,
            GetEventMetadataPayloadBatchFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_GetBlacklistJobExecutionFailed(object sender, GetBlacklistJobExecutionFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_GetWhitelistJobExecutionFailed(object sender, GetWhitelistJobExecutionFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_EventMetaAdded(object sender, EventMetaAddedEventArgs e)
        {
            // ignore
        }

        private void Instance_EventMetadataPublishJobExecutionFailed(object sender,
            EventMetadataPublishJobExecutionFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_DataTransmitted(object sender, EventTransmittedEventArgs e)
        {
            // ignore
        }

        private void Instance_CouldNotParseIpAddressHttpHeaderValues(object sender,
            CouldNotParseIpAddressHttpHeaderValuesEventArgs e)
        {
            // ignore
        }

        private void Instance_CouldNotGetIpAddressHttpHeaderValues(object sender,
            CouldNotGetIpAddressHttpHeaderValuesEventArgs e)
        {
            // ignore
        }

        private void Instance_CloudDatabaseConnectionFailed(object sender, CloudDatabaseConnectionFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_ClearCacheFailed(object sender, ClearCacheFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_BlacklistListUpdated(object sender, EventArgs e)
        {
            // ignore
        }

        private void Instance_BlacklistIpAddressAdded(object sender, IpAddressAddedEventArgs e)
        {
            // ignore
        }

        private void Instance_BlacklistGotLatestList(object sender, GotLatestListEventArgs e)
        {
            // ignore
        }

        private void Instance_BlacklistGetLatestListFailed(object sender, GetLatestListFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_BlacklistCouldNotParseIpAddress(object sender, CouldNotParseIpAddressEventArgs e)
        {
            // ignore
        }

        private void Instance_BlacklistAddIpAddressFailed(object sender, AddIpAddressFailedEventArgs e)
        {
            // ignore
        }

        private void Instance_AddEventMetaFailed(object sender, AddEventMetaFailedEventArgs e)
        {
            // ignore
        }
    }
}