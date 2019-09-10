using System;
using System.Threading.Tasks;
using Quartz;

namespace Bwrx.Api
{
    [DisallowConcurrentExecution]
    internal class GetWhitelistJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var whitelist = (Whitelist) dataMap[nameof(Whitelist)];

                var latestWhitelist = await whitelist.GetLatestIndividualAsync();
                var whitelistRanges = await whitelist.GetLatestRangesAsync();
                whitelist.UpDate(latestWhitelist, whitelistRanges);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Get latest whitelist background task execution failed.";
                throw new Exception(errorMessage, exception);
            }
        }
    }
}