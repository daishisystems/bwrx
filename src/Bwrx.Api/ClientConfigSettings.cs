namespace Bwrx.Api
{
    public class ClientConfigSettings
    {
        public string ProjectId { get; set; }
        public string PublisherTopicId { get; set; }
        public long ElementCountThreshold { get; set; } = 1000;
        public long RequestByteThreshold { get; set; } = 5242880; // MAX bytes 10485760 / 2
        public int EventPublishDelayThreshold { get; set; } = 3;
        public int GetBlacklistTimeInterval { get; set; } = 1;
        public int GetWhitelistTimeInterval { get; set; } = 1;
        public int MaxThreadCount { get; set; } = 10;
        public int PublishExecutionTimeInterval { get; set; } = 5;
        public int MaxQueueLength { get; set; } = 600000;
        public int MaxItemsToDequeue { get; set; } = 2250;
        public int BlockingHttpStatusCode { get; set; } = 403;
        public string IpAddressHeaderName { get; set; } = "NS_CLIENT_IP";
        public bool PassiveBlockingMode { get; set; }
        public bool UsegRpc { get; set; }
        public string CloudFunctionHttpBaseAddress { get; set; }
        public string BlacklistUri { get; set; }
        public string BlacklistCountUri { get; set; }
        public string BlacklistRangesUri { get; set; }
        public string WhitelistUri { get; set; }
        public string WhitelistCountUri { get; set; }
        public string WhitelistRangesUri { get; set; }
        public string HttpProxy { get; set; }
        public string CloudFunctionRequestUri { get; set; } = string.Empty;
        public int MaxNumIpAddressesPerHttpRequest { get; set; } = 250000;
    }
}