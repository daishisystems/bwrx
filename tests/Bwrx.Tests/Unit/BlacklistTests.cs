using System.Collections.Generic;
using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Unit
{
    public class BlacklistTests
    {
        [Fact]
        public void BlacklistIsUpdated()
        {
            Blacklist.Instance.UpDate(new List<string> {"1", "1"});
            Assert.Contains("1", Blacklist.Instance.IpAddresses);
            Assert.DoesNotContain("2", Blacklist.Instance.IpAddresses);
        }

        [Fact]
        public void IpAddressIsBlacklisted()
        {
            Blacklist.Instance.IpAddresses.Add("1");
            Assert.True(Blacklist.Instance.IsIpAddressBlacklisted("1"));
        }
    }
}