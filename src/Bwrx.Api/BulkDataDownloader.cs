using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bwrx.Api
{
    public class BulkDataDownloader
    {
        public BulkDataDownloader()
        {
        }

        public BulkDataDownloader(ClientConfigSettings clientConfigSettings)
        {
            if (clientConfigSettings == null) throw new ArgumentNullException(nameof(clientConfigSettings));
        }

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
    }

    public class RecordCount
    {
        public int Total { get; set; }
    }
}