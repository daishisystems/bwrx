using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace Bwrx.Api
{
    internal class GetWhitelistJobListener : IJobListener
    {
        public Task JobToBeExecuted(IJobExecutionContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (jobException != null)
                OnGetWhitelistJobExecutionFailed(
                    new GetWhitelistJobExecutionFailedEventArgs(jobException));
            return Task.CompletedTask;
        }

        public string Name => "getWhitelistJobListener";

        public event GetWhitelistJobExecutionFailedEventHandler GetWhitelistJobExecutionFailed;

        private void OnGetWhitelistJobExecutionFailed(GetWhitelistJobExecutionFailedEventArgs e)
        {
            GetWhitelistJobExecutionFailed?.Invoke(this, e);
        }
    }
}