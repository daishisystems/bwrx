using System;
using System.Linq;
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
                bulkDataDownloader.CalcNumHttpRequestsRequired(-1, 20);
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

        [Fact]
        public void PaginationSequenceIsCalculated()
        {
            var bulkDataDownloader = new BulkDataDownloader();
            var paginationSequence = bulkDataDownloader.CalcPaginationSequence(5, 10).ToList();

            Assert.Equal(0, paginationSequence[0].Item1);
            Assert.Equal(10, paginationSequence[0].Item2);
            Assert.Equal(10, paginationSequence[1].Item1);
            Assert.Equal(10, paginationSequence[1].Item2);
            Assert.Equal(20, paginationSequence[2].Item1);
            Assert.Equal(10, paginationSequence[2].Item2);
            Assert.Equal(30, paginationSequence[3].Item1);
            Assert.Equal(10, paginationSequence[3].Item2);
            Assert.Equal(40, paginationSequence[4].Item1);
            Assert.Equal(10, paginationSequence[4].Item2);
        }

        [Fact]
        public void RequestUriIsFormattedForPagination()
        {
            var bulkDataDownloader = new BulkDataDownloader();
            const string requestUri = "http://test.com";
            var formattedRequestUri = bulkDataDownloader.FormatRequestUriForPagination(requestUri, 0, 10);
            Assert.Equal("http://test.com?startpage=0&endpage=10", formattedRequestUri);
        }
    }
}