using System;
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
    }

    public class RecordCount
    {
        public int Total { get; set; }
    }
}