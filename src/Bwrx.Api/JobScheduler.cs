using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace Bwrx.Api
{
    public class JobScheduler
    {
        public delegate void JobSchedulerStartFailedEventHandler(object sender, JobSchedulerStartFailedEventArgs e);

        private static readonly Lazy<JobScheduler> InnerJobScheduler =
            new Lazy<JobScheduler>(() => new JobScheduler());

        private JobDetailImpl _eventMetadataPublishJobDetail;

        private ITrigger _eventMetadataPublishJobTrigger;

        private EventMetadataPublishJobListener _eventMetadataUploadJobListener;
        private JobDetailImpl _getBlacklistJobDetail;
        private GetBlacklistJobListener _getBlacklistJobListener;
        private ITrigger _getBlacklistJobTrigger;
        private JobDetailImpl _getWhitelistJobDetail;
        private GetWhitelistJobListener _getWhitelistJobListener;
        private ITrigger _getWhitelistJobTrigger;

        private IScheduler _scheduler;

        public static JobScheduler Instance => InnerJobScheduler.Value;

        public event JobSchedulerStartFailedEventHandler JobSchedulerStartFailed;

        public event EventMetadataPublishJobExecutionFailedEventHandler EventMetadataPublishJobExecutionFailed;

        public event GetBlacklistJobExecutionFailedEventHandler GetBlacklistJobExecutionFailed;

        public event GetWhitelistJobExecutionFailedEventHandler GetWhitelistJobExecutionFailed;

        public async Task StartAsync(
            EventTransmissionClient eventTransmissionClient,
            EventMetaCache eventMetaCache,
            Blacklist blacklist,
            Whitelist whitelist,
            ClientConfigSettings eventTransmissionClientConfigSettings)
        {
            if (eventTransmissionClient == null) throw new ArgumentNullException(nameof(eventTransmissionClient));
            if (eventMetaCache == null) throw new ArgumentNullException(nameof(eventMetaCache));
            if (blacklist == null) throw new ArgumentNullException(nameof(blacklist));
            if (whitelist == null) throw new ArgumentNullException(nameof(whitelist));
            if (eventTransmissionClientConfigSettings == null)
                throw new ArgumentNullException(nameof(eventTransmissionClientConfigSettings));

            try
            {
                eventMetaCache.MaxQueueLength = eventTransmissionClientConfigSettings.MaxQueueLength;
                eventMetaCache.MaxItemsToDequeue = eventTransmissionClientConfigSettings.MaxItemsToDequeue;

                var factory = new StdSchedulerFactory(new NameValueCollection
                {
                    ["quartz.threadPool.threadCount"] = eventTransmissionClientConfigSettings.MaxThreadCount.ToString()
                });
                _scheduler = await factory.GetScheduler();
                
                await _scheduler.Start();

                try
                {
                    await StartEventPublishJob(
                        eventTransmissionClient,
                        eventMetaCache,
                        eventTransmissionClientConfigSettings);
                }
                catch (Exception exception)
                {
                    OnEventMetadataPublishJobExecutionFailed(
                        new EventMetadataPublishJobExecutionFailedEventArgs(exception));
                    throw new Exception("Failed to start event-publish background task.", exception);
                }

                try
                {
                    await StartGetBlacklistJob(
                        blacklist,
                        whitelist,
                        eventTransmissionClientConfigSettings);
                }
                catch (Exception exception)
                {
                    OnGetBlacklistJobExecutionFailed(new GetBlacklistJobExecutionFailedEventArgs(exception));
                    throw new Exception("Failed to start get-blacklist background task.", exception);
                }
            }
            catch (Exception exception)
            {
                const string errorMessage = "Failed to start background tasks.";
                OnJobSchedulerStartFailed(new JobSchedulerStartFailedEventArgs(new Exception(errorMessage, exception)));
            }
        }

        private async Task StartGetBlacklistJob(
            Blacklist blacklist,
            Whitelist whitelist,
            ClientConfigSettings eventTransmissionClientConfigSettings)
        {
            const string blacklistJobName = "getBlacklistJob";

            _getBlacklistJobDetail = new JobDetailImpl(
                blacklistJobName,
                typeof(GetBlacklistJob))
            {
                JobDataMap =
                {
                    [nameof(Blacklist)] = blacklist,
                    [nameof(Whitelist)] = whitelist
                }
            };

            _getBlacklistJobListener = new GetBlacklistJobListener();

            if (GetBlacklistJobExecutionFailed != null)
                _getBlacklistJobListener.GetBlacklistJobExecutionFailed += GetBlacklistJobExecutionFailed;

            _scheduler.ListenerManager.AddJobListener(
                _getBlacklistJobListener,
                KeyMatcher<JobKey>.KeyEquals(new JobKey(blacklistJobName)));

            _getBlacklistJobTrigger = TriggerBuilder.Create()
                .WithSimpleSchedule(s => s
                    .WithIntervalInMinutes(eventTransmissionClientConfigSettings.GetBlacklistTimeInterval)
                    .RepeatForever())
                .Build();

            await _scheduler.ScheduleJob(_getBlacklistJobDetail, _getBlacklistJobTrigger);
        }

        private async Task StartGetWhitelistJob(
            BigQueryClient bigQueryClient,
            Whitelist whitelist,
            ClientConfigSettings eventTransmissionClientConfigSettings)
        {
            const string whitelistJobName = "getWhitelistJob";

            _getWhitelistJobDetail = new JobDetailImpl(
                whitelistJobName,
                typeof(GetWhitelistJob))
            {
                JobDataMap =
                {
                    [nameof(BigQueryClient)] = bigQueryClient,
                    [nameof(Whitelist)] = whitelist
                }
            };

            _getWhitelistJobListener = new GetWhitelistJobListener();

            if (GetWhitelistJobExecutionFailed != null)
                _getWhitelistJobListener.GetWhitelistJobExecutionFailed += GetWhitelistJobExecutionFailed;

            _scheduler.ListenerManager.AddJobListener(
                _getWhitelistJobListener,
                KeyMatcher<JobKey>.KeyEquals(new JobKey(whitelistJobName)));

            _getWhitelistJobTrigger = TriggerBuilder.Create()
                .WithSimpleSchedule(s => s
                    .WithIntervalInMinutes(eventTransmissionClientConfigSettings.GetWhitelistTimeInterval)
                    .RepeatForever())
                .Build();

            await _scheduler.ScheduleJob(_getWhitelistJobDetail, _getWhitelistJobTrigger);
        }

        private async Task StartEventPublishJob(
            EventTransmissionClient eventTransmissionClient,
            EventMetaCache eventMetaCache,
            ClientConfigSettings eventTransmissionClientConfigSettings)
        {
            const string publishJobName = "eventMetadataUploadJob";

            _eventMetadataPublishJobDetail = new JobDetailImpl(
                publishJobName,
                typeof(EventMetadataPublishJob))
            {
                JobDataMap =
                {
                    [nameof(EventTransmissionClient)] = eventTransmissionClient,
                    [nameof(EventMetaCache)] = eventMetaCache,
                    [nameof(ClientConfigSettings)] = eventTransmissionClientConfigSettings
                }
            };

            _eventMetadataUploadJobListener = new EventMetadataPublishJobListener();

            if (EventMetadataPublishJobExecutionFailed != null)
                _eventMetadataUploadJobListener.EventMetadataPublishJobExecutionFailed +=
                    EventMetadataPublishJobExecutionFailed;

            _scheduler.ListenerManager.AddJobListener(
                _eventMetadataUploadJobListener,
                KeyMatcher<JobKey>.KeyEquals(new JobKey(publishJobName)));

            _eventMetadataPublishJobTrigger = TriggerBuilder.Create()
                .WithSimpleSchedule(s => s
                    .WithIntervalInSeconds(eventTransmissionClientConfigSettings.PublishExecutionTimeInterval)
                    .RepeatForever())
                .Build();

            await _scheduler.ScheduleJob(_eventMetadataPublishJobDetail, _eventMetadataPublishJobTrigger);
        }

        private void OnGetBlacklistJobExecutionFailed(GetBlacklistJobExecutionFailedEventArgs e)
        {
            GetBlacklistJobExecutionFailed?.Invoke(this, e);
        }

        private void OnGetWhitelistJobExecutionFailed(GetWhitelistJobExecutionFailedEventArgs e)
        {
            GetWhitelistJobExecutionFailed?.Invoke(this, e);
        }

        private void OnJobSchedulerStartFailed(JobSchedulerStartFailedEventArgs e)
        {
            JobSchedulerStartFailed?.Invoke(this, e);
        }

        private void OnEventMetadataPublishJobExecutionFailed(
            EventMetadataPublishJobExecutionFailedEventArgs e)
        {
            EventMetadataPublishJobExecutionFailed?.Invoke(this, e);
        }
    }
}