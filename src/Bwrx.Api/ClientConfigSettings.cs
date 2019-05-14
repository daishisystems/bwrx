using System.Collections.Generic;

namespace Bwrx.Api
{
    public class ClientConfigSettings
    {
        public string ProjectId { get; set; }
        public string PublisherTopicId { get; set; }
        public string SubscriberTopicId { get; set; }
        public long ElementCountThreshold { get; set; } = 1000;
        public long RequestByteThreshold { get; set; } = 5242880; // MAX bytes 10485760 / 2
        public int EventPublishDelayThreshold { get; set; } = 3;
        public int GetBlacklistTimeInterval { get; set; } = 5;
        public int GetWhitelistTimeInterval { get; set; } = 5;
        public int MaxThreadCount { get; set; } = 10;
        public int PublishExecutionTimeInterval { get; set; } = 30;
        public int MaxQueueLength { get; set; } = 175000;
        public IEnumerable<string> EndpointsToMonitor { get; set; }
        public int BlockingHttpStatusCode { get; set; } = 403;
        public string IpAddressHeaderName { get; set; }
        public bool PassiveBlockingMode { get; set; }
        public bool UsegRpc { get; set; }
        public string CloudFunctionHttpBaseAddress { get; set; }
        public string HttpProxy { get; set; }
        public string CloudFunctionRequestUri { get; set; } = string.Empty;
    }
}