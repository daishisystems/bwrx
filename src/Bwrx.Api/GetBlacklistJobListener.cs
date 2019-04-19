using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace Bwrx.Api
{
    internal class GetBlacklistJobListener : IJobListener
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
                OnGetBlacklistJobExecutionFailed(
                    new GetBlacklistJobExecutionFailedEventArgs(jobException));
            return Task.CompletedTask;
        }

        public string Name => "getBlacklistJobListener";

        public event GetBlacklistJobExecutionFailedEventHandler GetBlacklistJobExecutionFailed;

        private void OnGetBlacklistJobExecutionFailed(GetBlacklistJobExecutionFailedEventArgs e)
        {
            GetBlacklistJobExecutionFailed?.Invoke(this, e);
        }
    }
}