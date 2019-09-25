using System.Collections.Generic;
using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Unit
{
    public class IpAddressRangeTests
    {
        [Fact]
        public void InvalidIpAddressRangeThrowsException()
        {
            const string ipAddressRange = "52.93.34.57/X";
            const string ipAddress = "52.93.34.75";

            Assert.False(Blacklist.Instance.IpAddressIsInRange(ipAddress, ipAddressRange));
        }

        [Fact]
        public void InvalidIpAddressThrowsException()
        {
            const string ipAddressRange = "52.93.34.57/16";
            const string ipAddress = "52.93.34.X";

            Assert.False(Blacklist.Instance.IpAddressIsInRange(ipAddress, ipAddressRange));
        }

        [Fact]
        public void IpAddressIsInRange1()
        {
            const string ipAddressRange = "52.93.34.57/16";
            const string ipAddress = "52.93.34.75";

            var isInRange = Blacklist.Instance.IpAddressIsInRange(ipAddress, ipAddressRange);
            Assert.True(isInRange);
        }

        [Fact]
        public void IpAddressIsInRange2()
        {
            const string ipAddressRange = "52.93.34.57-52.93.255.255";
            const string ipAddress = "52.93.34.75";

            var isInRange = Blacklist.Instance.IpAddressIsInRange(ipAddress, ipAddressRange);
            Assert.True(isInRange);
        }

        [Fact]
        public void IpAddressIsInRanges()
        {
            const string ipAddress = "52.93.34.75";
            var ipAddressRanges = new List<string>
            {
                "52.93.34.57/32",
                "52.93.34.57/16"
            };

            var ipIsInRange = Blacklist.Instance.IpAddressIsInRanges(ipAddress, ipAddressRanges, out var ipRangeIndex);
            Assert.True(ipIsInRange);
            Assert.Equal(1, ipRangeIndex);
        }

        [Fact]
        public void IpAddressIsNotInRange1()
        {
            const string ipAddressRange = "52.93.34.57/32";
            const string ipAddress = "52.93.34.75";

            var isInRange = Blacklist.Instance.IpAddressIsInRange(ipAddress, ipAddressRange);
            Assert.False(isInRange);
        }

        [Fact]
        public void IpAddressIsNotInRange2()
        {
            const string ipAddressRange = "52.93.34.57-52.93.34.57";
            const string ipAddress = "52.93.34.75";

            var isInRange = Blacklist.Instance.IpAddressIsInRange(ipAddress, ipAddressRange);
            Assert.False(isInRange);
        }

        [Fact]
        public void IpAddressIsNotInRanges()
        {
            const string ipAddress = "192.168.0.1";
            var ipAddressRanges = new List<string>
            {
                "52.93.34.57/32",
                "52.93.34.57/16"
            };

            var ipIsInRange = Blacklist.Instance.IpAddressIsInRanges(ipAddress, ipAddressRanges, out var ipRangeIndex);
            Assert.False(ipIsInRange);
            Assert.Equal(1, ipRangeIndex);
        }

        [Fact]
        public void NullIpAddressRangeThrowsException()
        {
            const string ipAddress = "52.93.34.75";

            Assert.False(Blacklist.Instance.IpAddressIsInRange(ipAddress, null));
        }

        [Fact]
        public void NullIpAddressThrowsException()
        {
            const string ipAddressRange = "52.93.34.57/16";

            Assert.False(Blacklist.Instance.IpAddressIsInRange(null, ipAddressRange));
        }
    }
}