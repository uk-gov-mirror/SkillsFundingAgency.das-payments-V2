using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Monitoring.Jobs.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Application.JobProcessing;
using SFA.DAS.Payments.Monitoring.Jobs.Data;
using SFA.DAS.Payments.Monitoring.Jobs.Model;
using SFA.DAS.Payments.ServiceFabric.Core;
using SFA.DAS.Payments.ServiceFabric.Core.Batch;

namespace SFA.DAS.Payments.Monitoring.Jobs.JobService
{
    public class JobStorageService : IJobStorageService
    {
        private readonly IJobsDataContext dataContext;
        private readonly IPaymentLogger logger;
        private readonly IJobStatusManager jobStatusManager;

        public const string JobCacheKey = "jobs";
        public const string JobStatusCacheKey = "job_status";
        public const string InProgressMessagesCacheKey = "inprogress_messages";
        public const string CompletedMessagesCacheKey = "completed_messages";

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<long, JobModel>> JobCache =
            new ConcurrentDictionary<string, ConcurrentDictionary<long, JobModel>>();

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, InProgressMessage>> InProgressMessageCache =
            new ConcurrentDictionary<string, ConcurrentDictionary<Guid, InProgressMessage>>();

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, CompletedMessage>> CompletedMessageCache =
            new ConcurrentDictionary<string, ConcurrentDictionary<Guid, CompletedMessage>>();

        private static readonly ConcurrentDictionary<long, (bool hasFailedMessages, DateTimeOffset? endTime)> JobStatusCache =
            new ConcurrentDictionary<long, (bool hasFailedMessages, DateTimeOffset? endTime)>();

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, InProgressMessage>> DatalockMessageCache =
            new ConcurrentDictionary<string, ConcurrentDictionary<Guid, InProgressMessage>>();


        public JobStorageService(IReliableStateManagerProvider stateManagerProvider,
            IReliableStateManagerTransactionProvider reliableTransactionProvider,
            IJobsDataContext dataContext, 
            IPaymentLogger logger, 
            IJobStatusManager jobStatusManager)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.jobStatusManager = jobStatusManager;
        }
        private static string GetCacheKey(string cacheKeyPrefix, long jobId) => $"{cacheKeyPrefix}_{jobId}";

        private ConcurrentDictionary<long, JobModel> GetJobCollection()
        {
            if (JobCache.TryGetValue(JobCacheKey, out var value))
            {
                return value;
            }
            var newValue = new ConcurrentDictionary<long, JobModel>();
            JobCache.TryAdd(JobCacheKey, newValue);
            return newValue;
        }

        public async Task<bool> StoreNewJob(JobModel job, CancellationToken cancellationToken)
        {
            if (!job.DcJobId.HasValue)
                throw new InvalidOperationException($"No dc job id specified for the job. Job type: {job.JobType:G}");
            cancellationToken.ThrowIfCancellationRequested();
            var jobCache = GetJobCollection();
            if (jobCache.TryGetValue(job.DcJobId.Value, out var cachedJob))
            {
                logger.LogDebug($"Job has already been stored.");
                return false;
            }

            jobCache.AddOrUpdate(job.DcJobId.Value, id => job, (id, existingJob) => job);

            if (job.Id == 0)
                await dataContext.SaveNewJob(job, cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task SaveJobStatus(long jobId, JobStatus jobStatus, DateTimeOffset endTime, CancellationToken cancellationToken)
        {
            var job = await GetJob(jobId, cancellationToken).ConfigureAwait(false);
            if (job == null)
                throw new InvalidOperationException($"Job not stored in the cache. Job: {jobId}");
            job.Status = jobStatus;
            job.EndTime = endTime;

            var jobCache = GetJobCollection();
            jobCache.AddOrUpdate(jobId, job, (key, value) => job);
            await dataContext.SaveJobStatus(jobId, jobStatus, endTime, cancellationToken).ConfigureAwait(false);
        }

        public Task<JobModel> GetJob(long jobId, CancellationToken cancellationToken)
        {
            var collection = GetJobCollection();

            if (collection.TryGetValue(jobId, out var item))
            {
                return Task.FromResult(item);
            }

            return Task.FromResult<JobModel>(null);
        }

        private async Task<ConcurrentDictionary<Guid, InProgressMessage>> GetInProgressMessagesCollection(long jobId)
        {
            var key = $"{InProgressMessagesCacheKey}_{jobId}";
            if (InProgressMessageCache.TryGetValue(key, out var value))
            {
                return value;
            }
            var newValue = new ConcurrentDictionary<Guid, InProgressMessage>();
            InProgressMessageCache.TryAdd(key, newValue);
            return newValue;
        }

        private ConcurrentDictionary<Guid, InProgressMessage> GetDatalockCollection(long jobId)
        {
            var key = $"{InProgressMessagesCacheKey}_{jobId}";
            if (DatalockMessageCache.TryGetValue(key, out var value))
            {
                return value;
            }
            var newValue = new ConcurrentDictionary<Guid, InProgressMessage>();
            DatalockMessageCache.TryAdd(key, newValue);
            return newValue;
        }

        public async Task<List<InProgressMessage>> GetInProgressMessages(long jobId, CancellationToken cancellationToken)
        {
            var inProgressCollection = await GetInProgressMessagesCollection(jobId).ConfigureAwait(false);
            return inProgressCollection.Values.ToList();
        }

        public async Task RemoveInProgressMessages(long jobId, List<Guid> messageIdentifiers, CancellationToken cancellationToken)
        {
            var inProgressCollection = await GetInProgressMessagesCollection(jobId).ConfigureAwait(false);
            foreach (var messageIdentifier in messageIdentifiers)
            {
                inProgressCollection.TryRemove(messageIdentifier, out _);
            }
        }


        private static readonly List<string> DataLocksMessages = new List<string> {
            "FunctionalSkillEarningFailedDataLockMatching",
            "PayableFunctionalSkillEarningEvent",
            "PayableEarningEvent",
            "EarningFailedDataLockMatching",
            "ProcessLearnerCommand",
            "Act1FunctionalSkillEarningsEvent",
            "ApprenticeshipContractType1EarningEvent"
        };
        private bool IsDatalockMessage(string messageName)
        {
            return DataLocksMessages.Any(x => messageName.Contains(x));
        }

        public async Task StoreInProgressMessages(long jobId, List<InProgressMessage> inProgressMessages, CancellationToken cancellationToken)
        {
            var inProgressMessagesCollection = await GetInProgressMessagesCollection(jobId);
            var datalockMessageCollection = GetDatalockCollection(jobId);

            foreach (var inProgressMessage in inProgressMessages)
            {
                inProgressMessagesCollection.TryAdd(inProgressMessage.MessageId, inProgressMessage);
                if (IsDatalockMessage(inProgressMessage.MessageName))
                {
                    datalockMessageCollection.TryAdd(inProgressMessage.MessageId, inProgressMessage);
                }
            }
        }

        private async Task<ConcurrentDictionary<Guid, CompletedMessage>> GetCompletedMessagesCollection(long jobId)
        {
            var key = $"{CompletedMessagesCacheKey}_{jobId}";

            if (CompletedMessageCache.TryGetValue(key, out var value))
            {
                return value;
            }
            var newValue = new ConcurrentDictionary<Guid, CompletedMessage>();
            CompletedMessageCache.TryAdd(key, newValue);
            return newValue;
        }

        public async Task<List<CompletedMessage>> GetCompletedMessages(long jobId, CancellationToken cancellationToken)
        {
            var completedMessageCollection = await GetCompletedMessagesCollection(jobId).ConfigureAwait(false);
            return completedMessageCollection.Values.ToList();
        }

        public async Task RemoveCompletedMessages(long jobId, List<Guid> completedMessages, CancellationToken cancellationToken)
        {
            var completedMessagesCollection = await GetCompletedMessagesCollection(jobId).ConfigureAwait(false);
            foreach (var completedMessage in completedMessages)
            {
                completedMessagesCollection.TryRemove(completedMessage, out _);
            }
        }

        public async Task StoreCompletedMessage(CompletedMessage completedMessage, CancellationToken cancellationToken)
        {
            var inProgressMessages = await GetInProgressMessagesCollection(completedMessage.JobId);
            var datalockMessages = GetDatalockCollection(completedMessage.JobId);

            inProgressMessages.TryRemove(completedMessage.MessageId, out _);
            
            if (inProgressMessages.Count == 0)
            {
                await SaveJobStatus(completedMessage.JobId, JobStatus.Completed, completedMessage.CompletedTime,
                    default);
            }

            if (datalockMessages.TryRemove(completedMessage.MessageId, out var value))
            {
                if (value != null && datalockMessages.Count == 0)
                {
                    await SaveDataLocksCompletionTime(completedMessage.JobId, completedMessage.CompletedTime, default);
                }
            }
            

            //cancellationToken.ThrowIfCancellationRequested();
            //var completedMessagesCollection =
            //    await GetCompletedMessagesCollection(completedMessage.JobId).ConfigureAwait(false);
            //await completedMessagesCollection.AddOrUpdateAsync(reliableTransactionProvider.Current,
            //        completedMessage.MessageId,
            //        completedMessage, (key, value) => completedMessage, TransactionTimeout, cancellationToken)
            //    .ConfigureAwait(false);
        }


        public async Task<(bool hasFailedMessages, DateTimeOffset? endTime)> GetJobStatus(long jobId, CancellationToken cancellationToken)
        {
            if (JobStatusCache.TryGetValue(jobId, out var value))
            {
                return value;
            }
            return (true, null);
        }

        public async Task StoreJobStatus(long jobId, bool hasFailedMessages, DateTimeOffset? endTime, CancellationToken cancellationToken)
        {
            JobStatusCache.AddOrUpdate(jobId, (hasFailedMessages: hasFailedMessages, endTime: endTime),
                (key, value) => (hasFailedMessages: hasFailedMessages, endTime: endTime));
        }

        public async Task SaveDataLocksCompletionTime(long jobId, DateTimeOffset endTime, CancellationToken cancellationToken)
        {
            await dataContext.SaveDataLocksCompletionTime(jobId, endTime, cancellationToken).ConfigureAwait(
                false);
        }
    }
}