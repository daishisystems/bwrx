using System.Collections.Generic;
using System.Net;
using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Unit
{
    public class BlacklistTests
    {
        [Fact]
        public void BlacklistIsUpdated()
        {
            Blacklist.Instance.AddIPAddress(new IPAddress(1), new List<IPAddress>());
            Blacklist.Instance.UpDate(new List<IPAddress> {new IPAddress(2)});
            Assert.Contains(new IPAddress(2), Blacklist.Instance.IpAddresses);
            Assert.DoesNotContain(new IPAddress(1), Blacklist.Instance.IpAddresses);
        }

        [Fact]
        public void ExistingIpAddressIsNotAdded()
        {
            Blacklist.Instance.AddIPAddress(new IPAddress(1));
            Assert.False(Blacklist.Instance.AddIPAddress(new IPAddress(1)));
        }

        [Fact]
        public void IpAddressIsAdded()
        {
            Assert.True(Blacklist.Instance.AddIPAddress(new IPAddress(1)));
        }

        [Fact]
        public void IpAddressIsBlacklisted()
        {
            Blacklist.Instance.AddIPAddress(new IPAddress(1));
            Assert.True(Blacklist.Instance.IsIpAddressBlacklisted(new IPAddress(1)));
        }

        [Fact]
        public void WhiteListedIpAddressIsNotAdded()
        {
            Assert.False(Blacklist.Instance.AddIPAddress(new IPAddress(1),
                new List<IPAddress>(new[] {new IPAddress(1)})));
        }
    }
}