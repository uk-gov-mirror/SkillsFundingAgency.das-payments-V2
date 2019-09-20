using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Monitoring.Jobs.Application.JobProcessing;
using SFA.DAS.Payments.Monitoring.Jobs.JobService.Interfaces;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Monitoring.Jobs.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IPaymentLogger logger;
        private readonly IEarningsJobService earningsJobService;
        private readonly IJobMessageService jobMessageService;

        public JobService(IPaymentLogger logger,
            IEarningsJobService earningsJobService,
            IJobMessageService jobMessageService)

        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.earningsJobService = earningsJobService;
            this.jobMessageService = jobMessageService;
        }

        public async Task RecordEarningsJob(RecordEarningsJob message, CancellationToken cancellationToken)
        {
            try
            {
                await earningsJobService.RecordNewJob(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Failed to record earnings job: {message.JobId}. Error: {ex.Message}, {ex}");
                throw;
            }
        }

        public async Task RecordEarningsJobAdditionalMessages(RecordEarningsJobAdditionalMessages message,
            CancellationToken cancellationToken)
        {
            try
            {
                await earningsJobService.RecordNewJobAdditionalMessages(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Failed to record earnings job: {message.JobId}. Error: {ex.Message}, {ex}");
                throw;
            }
        }

        public async Task RecordJobMessageProcessingStatus(RecordJobMessageProcessingStatus message, CancellationToken cancellationToken)
        {
            try
            {
                await jobMessageService.RecordCompletedJobMessageStatus(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Failed to record earnings job: {message.JobId}. Error: {ex.Message}, {ex}");
                throw;
            }
        }


        public Task RecordJobMessageProcessingStartedStatus(RecordStartedProcessingJobMessages message,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

