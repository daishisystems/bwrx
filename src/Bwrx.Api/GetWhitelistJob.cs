using System;
using System.Linq;
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

                var latestWhitelist = await whitelist.GetLatestAsync();
                whitelist.UpDate(latestWhitelist.ToList());
            }
            catch (Exception exception)
            {
                const string errorMessage = "Get latest whitelist background task execution failed.";
                throw new Exception(errorMessage, exception);
            }
        }
    }
}