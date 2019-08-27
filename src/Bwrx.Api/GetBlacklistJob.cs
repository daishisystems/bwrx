using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace Bwrx.Api
{
    [DisallowConcurrentExecution]
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

                var latestBlacklist = await blacklist.GetLatestAsync(whitelist.IpAddresses);
                blacklist.UpDate(latestBlacklist); // todo: Allow IP ranges
            }
            catch (Exception exception)
            {
                const string errorMessage = "Get latest blacklist background task execution failed.";
                throw new Exception(errorMessage, exception);
            }
        }
    }
}