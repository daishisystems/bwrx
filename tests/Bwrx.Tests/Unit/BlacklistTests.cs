using System.Collections.Generic;
using System.Net;
using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Unit
{
    public class BlacklistTests
    {
        [Fact]
        public void ExistingIpAddressIsNotAdded()
        {
            Blacklist.Instance.AddIPAddress(new IPAddress(1), new List<IPAddress>());
            Assert.False(Blacklist.Instance.AddIPAddress(new IPAddress(1), new List<IPAddress>()));
        }

        [Fact]
        public void IpAddressIsAdded()
        {
            Assert.True(Blacklist.Instance.AddIPAddress(new IPAddress(1), new List<IPAddress>()));
        }

        [Fact]
        public void WhiteListedIpAddressIsNotAdded()
        {
            Assert.False(Blacklist.Instance.AddIPAddress(new IPAddress(1),
                new List<IPAddress>(new[] {new IPAddress(1)})));
        }
    }
}