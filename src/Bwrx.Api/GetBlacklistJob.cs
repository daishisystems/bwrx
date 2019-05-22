using System;
using System.Linq;
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
                var blacklist = (Blacklist) dataMap[nameof(Blacklist)];
                var whitelist = (Whitelist) dataMap[nameof(Whitelist)];

                var latestWhitelist = await whitelist.GetLatestAsync();
                whitelist.UpDate(latestWhitelist.ToList());

                var latestBlacklist = await blacklist.GetLatestAsync(whitelist.IpAddressIndex);
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