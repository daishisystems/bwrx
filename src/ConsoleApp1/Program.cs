using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Bwrx.Api;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Working ...");

            const int maxNumRecordsPerHttpRequest = 250000;
            var bulkDataDownloader = new BulkDataDownloader();

            using (var httpClient = new HttpClient())
            {
                var recordCount = bulkDataDownloader.GetRecordCountAsync(httpClient,
                    "https://europe-west2-bwrx-dev.cloudfunctions.net/get-record-count-0?tablename=whitelist").Result;

                var numHttpRequestsRequired =
                    bulkDataDownloader.CalcNumHttpRequestsRequired(recordCount.Total, maxNumRecordsPerHttpRequest);
                var paginationSequence =
                    bulkDataDownloader.CalcPaginationSequence(numHttpRequestsRequired, maxNumRecordsPerHttpRequest);

                const string requestUri = "https://us-central1-bwrx-dev.cloudfunctions.net/function-1";
                var data = bulkDataDownloader.LoadDataAsync<IpAddressMeta>(httpClient, requestUri, paginationSequence)
                    .Result;

                stopwatch.Stop();
                Console.WriteLine(data.Count());
                Console.WriteLine(stopwatch.ElapsedMilliseconds);

                Console.ReadLine();
            }
        }
    }

    internal class IpAddressMeta
    {
        public string IpAddress { get; set; }
    }
}