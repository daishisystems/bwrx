using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Integration
{
    public class BulkRecordDownloaderTests
    {
        [Fact]
        public void GetPaginatedResults()
        {
            var bulkDataDownloader = new BulkDataDownloader();
            const string requestUri = "https://europe-west2-bwrx-dev.cloudfunctions.net/bwrx-dev-blacklist";
            var paginationSequence = bulkDataDownloader.CalcPaginationSequence(10, 100);
            List<IpAddressMeta> ipAddressMeta;

            using (var httpClient = new HttpClient())
            {
                ipAddressMeta = bulkDataDownloader
                    .LoadDataAsync<IpAddressMeta>(httpClient, requestUri, paginationSequence).Result.ToList();
            }

            Assert.Equal(1000, ipAddressMeta.Count);
        }

        [Fact]
        public void GetRecordCount()
        {
            var bulkDataDownloader = new BulkDataDownloader();

            using (var httpClient = new HttpClient())
            {
                var recordCountMeta = bulkDataDownloader.GetRecordCountAsync(httpClient,
                    "https://us-central1-bwrx-dev.cloudfunctions.net/record-count-test-0").Result;
                Assert.Equal(8, recordCountMeta.Total);
            }
        }
    }

    internal class IpAddressMeta
    {
        public string IpAddress { get; set; }
    }
}