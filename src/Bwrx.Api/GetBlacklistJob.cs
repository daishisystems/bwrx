using System;
using System.Net;
using System.Threading.Tasks;
using NetTools;
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
                var latestWhitelist = await whitelist.GetLatestIndividualAsync();
                var whitelistRanges = await whitelist.GetLatestRangesAsync();

                foreach (var whitelistRange in whitelistRanges)
                {
                    bool canParse = IPAddressRange.TryParse(whitelistRange, out _);
                    if (!canParse) throw new Exception("Invalid IP");
                }

                foreach (var ipaddress in latestWhitelist)
                {
                    bool canParse = IPAddress.TryParse(ipaddress, out _);
                    if (!canParse) throw new Exception("Invalid IP");
                }

                whitelist.UpDate(latestWhitelist, whitelistRanges);
                var latestBlacklist = await blacklist.GetLatestIndividualAsync();
                var blacklistRanges = await blacklist.GetLatestRangesAsync();

                foreach (var blacklistRange in blacklistRanges)
                {
                    bool canParse = IPAddressRange.TryParse(blacklistRange, out _);
                    if (!canParse) throw new Exception("Invalid IP");
                }

                foreach (var ipaddress in latestBlacklist)
                {
                    bool canParse = IPAddress.TryParse(ipaddress, out _);
                    if (!canParse) throw new Exception("Invalid IP");
                }

                blacklist.UpDate(latestBlacklist, blacklistRanges);
            }
            catch (Exception exception)
            {
                const string errorMessage = "Get latest blacklist background task execution failed.";
                throw new Exception(errorMessage, exception);
            }
        }
    }
}