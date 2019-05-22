using Newtonsoft.Json;

namespace Bwrx.Api
{
    internal class ListIpAddress
    {
        [JsonProperty("ipaddress")] public string IpAddress { get; set; }
    }
}