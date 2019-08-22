using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bwrx.Api
{
    public class BulkDataDownloader
    {
        public async Task<RecordCount> GetRecordCount(HttpClient httpClient, string requestUri)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (string.IsNullOrEmpty(requestUri)) throw new ArgumentNullException(nameof(requestUri));

            try
            {
                var httpResponse = await httpClient.GetStringAsync(requestUri);
                return JsonConvert.DeserializeObject<RecordCount>(httpResponse);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to get record-count", e);
            }
        }

        public int CalcNumHttpRequestsRequired(int numRecords, int maxNumRecordsPerHttpRequest)
        {
            if (numRecords <= 0) throw new ArgumentOutOfRangeException(nameof(numRecords));
            if (maxNumRecordsPerHttpRequest <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxNumRecordsPerHttpRequest));
            return numRecords / maxNumRecordsPerHttpRequest + 1;
        }

        // todo: Guard this
        public IEnumerable<Tuple<int, int>> CalcPaginationSequence(
            int numHttpRequestsRequired,
            int maxNumRecordsPerHttpRequest)
        {
            if (numHttpRequestsRequired < 0) throw new ArgumentOutOfRangeException(nameof(numHttpRequestsRequired));
            if (maxNumRecordsPerHttpRequest < 0)
                throw new ArgumentOutOfRangeException(nameof(maxNumRecordsPerHttpRequest));
            var paginationSequences = new List<Tuple<int, int>>();
            var previousIndex = 1;
            for (var i = 0; i < numHttpRequestsRequired; i++)
            {
                var paginationSequence = new Tuple<int, int>(previousIndex, maxNumRecordsPerHttpRequest);
                previousIndex += maxNumRecordsPerHttpRequest;
                paginationSequences.Add(paginationSequence);
            }

            return paginationSequences;
        }
    }

    public class RecordCount
    {
        public int Total { get; set; }
    }
}