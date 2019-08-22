using System.Net.Http;
using Bwrx.Api;
using Xunit;

namespace Bwrx.Tests.Integration
{
    public class BulkRecordDownloaderTests
    {
        [Fact]
        public void CanGetRecordCount()
        {
            var bulkDataDownloader = new BulkDataDownloader();

            using (var httpClient = new HttpClient())
            {
                var recordCountMeta = bulkDataDownloader.GetRecordCount(httpClient,
                    "https://us-central1-bwrx-dev.cloudfunctions.net/record-count-test-0").Result;
                Assert.Equal(8, recordCountMeta.Total);
            }
        }
    }
}