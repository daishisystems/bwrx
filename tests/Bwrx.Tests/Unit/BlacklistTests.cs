using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Unit
{
    public class BlacklistTests
    {
        [Fact]
        public void IpAddressIsBlacklistedByRange()
        {
            const string ipAddress = "52.93.34.75";
            Blacklist.Instance.IpAddressRanges.Add("52.93.34.57/16");

            var blacklisted = Blacklist.Instance.IsIpAddressBlacklisted(ipAddress);
            Assert.True(blacklisted);
            Blacklist.Instance.IpAddressRanges.Clear();
        }

        [Fact]
        public void IpAddressIsBlacklistedIndividually()
        {
            const string ipAddress = "192.168.0.1";
            Blacklist.Instance.IpAddresses.Add(ipAddress);

            var blacklisted = Blacklist.Instance.IsIpAddressBlacklisted(ipAddress);
            Assert.True(blacklisted);
            Blacklist.Instance.IpAddresses.Clear();
        }

        [Fact]
        public void IpAddressIsNotBlacklisted1()
        {
            const string ipAddress = "192.168.0.1";
            Whitelist.Instance.IpAddresses.Add(ipAddress);

            var blacklisted = Blacklist.Instance.IsIpAddressBlacklisted(ipAddress);
            Assert.False(blacklisted);
            Whitelist.Instance.IpAddresses.Clear();
        }

        [Fact]
        public void IpAddressIsNotBlacklisted2()
        {
            const string ipAddress = "192.168.0.1";
            Whitelist.Instance.IpAddresses.Add(ipAddress);
            Blacklist.Instance.IpAddresses.Add(ipAddress);

            var blacklisted = Blacklist.Instance.IsIpAddressBlacklisted(ipAddress);
            Assert.False(blacklisted);
            Whitelist.Instance.IpAddresses.Clear();
            Blacklist.Instance.IpAddresses.Clear();
        }
    }
}