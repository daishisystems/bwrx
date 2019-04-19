using System;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;
using Quartz;

namespace Bwrx.Api
{
    internal class GetBlacklistJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var bigQueryClient = (BigQueryClient) dataMap[nameof(BigQueryClient)];
                var blacklist = (Blacklist) dataMap[nameof(Blacklist)];

                var latestBlacklist = await blacklist.GetLatestAsync(bigQueryClient);
                blacklist.UpDate(latestBlacklist);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Get latest blacklist background task execution failed.";
                throw new Exception(errorMessage, exception);
            }
        }
    }
}