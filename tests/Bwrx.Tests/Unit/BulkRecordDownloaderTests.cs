using System;
using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Unit
{
    public class BulkRecordDownloaderTests
    {
        [Fact]
        public void CanCalcNumHttpRequestsRequired0()
        {
            var bulkDataDownloader = new BulkDataDownloader();
            var numHttpRequestsRequired = bulkDataDownloader.CalcNumHttpRequestsRequired(10, 3);
            Assert.Equal(4, numHttpRequestsRequired);
        }

        [Fact]
        public void CanCalcNumHttpRequestsRequired1()
        {
            var bulkDataDownloader = new BulkDataDownloader();
            var numHttpRequestsRequired = bulkDataDownloader.CalcNumHttpRequestsRequired(10, 20);
            Assert.Equal(1, numHttpRequestsRequired);
        }

        [Fact]
        public void CanCalcNumHttpRequestsRequired2()
        {
            var bulkDataDownloader = new BulkDataDownloader();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                bulkDataDownloader.CalcNumHttpRequestsRequired(0, 20);
            });
        }

        [Fact]
        public void CanCalcNumHttpRequestsRequired3()
        {
            var bulkDataDownloader = new BulkDataDownloader();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                bulkDataDownloader.CalcNumHttpRequestsRequired(10, 0);
            });
        }
    }
}