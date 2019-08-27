using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bwrx.Api
{
    public class BulkDataDownloader
    {
        public async Task<RecordCount> GetRecordCountAsync(HttpClient httpClient, string requestUri)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (string.IsNullOrEmpty(requestUri)) throw new ArgumentNullException(nameof(requestUri));

            try
            {
                var httpResponse = await httpClient.GetStringAsync(requestUri);
                var recordCount = JsonConvert.DeserializeObject<RecordCount[]>(httpResponse);
                return recordCount.First();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to get record-count", e);
            }
        }

        public int CalcNumHttpRequestsRequired(int numRecords, int maxNumRecordsPerHttpRequest)
        {
            if (numRecords < 0) throw new ArgumentOutOfRangeException(nameof(numRecords));
            if (maxNumRecordsPerHttpRequest <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxNumRecordsPerHttpRequest));
            return numRecords / maxNumRecordsPerHttpRequest + 1;
        }

        public IEnumerable<Tuple<int, int>> CalcPaginationSequence(
            int numHttpRequestsRequired,
            int maxNumRecordsPerHttpRequest)
        {
            if (numHttpRequestsRequired < 0) throw new ArgumentOutOfRangeException(nameof(numHttpRequestsRequired));
            if (maxNumRecordsPerHttpRequest < 0)
                throw new ArgumentOutOfRangeException(nameof(maxNumRecordsPerHttpRequest));
            var paginationSequences = new List<Tuple<int, int>>();
            var previousIndex = 0;
            for (var i = 0; i < numHttpRequestsRequired; i++)
            {
                var paginationSequence = new Tuple<int, int>(previousIndex, maxNumRecordsPerHttpRequest);
                previousIndex += maxNumRecordsPerHttpRequest;
                paginationSequences.Add(paginationSequence);
            }

            return paginationSequences;
        }

        public async Task<IEnumerable<T>> LoadDataAsync<T>(
            HttpClient httpClient,
            string requestUri,
            IEnumerable<Tuple<int, int>> paginationSequence)
        {
            if (httpClient == null) throw new ArgumentException(nameof(httpClient));
            if (string.IsNullOrEmpty(requestUri)) throw new ArgumentNullException(nameof(requestUri));
            if (paginationSequence == null) throw new ArgumentNullException(nameof(paginationSequence));
            var dataItems = new List<T>();

            foreach (var sequence in paginationSequence)
            {
                var formattedRequestUri = FormatRequestUriForPagination(requestUri, sequence.Item1, sequence.Item2);
                var httpResponse = await httpClient.GetStringAsync(formattedRequestUri);
                var payload = JsonConvert.DeserializeObject<IEnumerable<T>>(httpResponse);
                dataItems.AddRange(payload);
            }

            return dataItems;
        }

        public string FormatRequestUriForPagination(string requestUri, int paginationStartIndex, int paginationEndIndex)
        {
            if (string.IsNullOrEmpty(requestUri)) throw new ArgumentNullException(nameof(requestUri));
            if (paginationStartIndex < 0) throw new ArgumentNullException(nameof(paginationStartIndex));
            if (paginationEndIndex < 0) throw new ArgumentNullException(nameof(paginationEndIndex));
            return requestUri + "?startpage=" + paginationStartIndex + "&endpage=" + paginationEndIndex;
        }
    }

    public class RecordCount
    {
        public int Total { get; set; }
    }
}