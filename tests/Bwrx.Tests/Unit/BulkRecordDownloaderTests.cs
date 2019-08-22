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

        // todo: Test guards
        [Fact]
        public void PaginationSequenceIsCalculated()
        {
            var bulkDataDownloader = new BulkDataDownloader();
            var paginationSequence = bulkDataDownloader.CalcPaginationSequence(5, 10).ToList();

            Assert.Equal(1, paginationSequence[0].Item1);
            Assert.Equal(10, paginationSequence[0].Item2);
            Assert.Equal(11, paginationSequence[1].Item1);
            Assert.Equal(10, paginationSequence[1].Item2);
            Assert.Equal(21, paginationSequence[2].Item1);
            Assert.Equal(10, paginationSequence[2].Item2);
            Assert.Equal(31, paginationSequence[3].Item1);
            Assert.Equal(10, paginationSequence[3].Item2);
            Assert.Equal(41, paginationSequence[4].Item1);
            Assert.Equal(10, paginationSequence[4].Item2);
        }
    }
}