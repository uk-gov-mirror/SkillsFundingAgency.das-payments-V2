using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Monitoring.Jobs.Application.Infrastructure.Configuration;
using SFA.DAS.Payments.Monitoring.Jobs.Model;

namespace SFA.DAS.Payments.Monitoring.Jobs.Application.JobProcessing.PeriodEnd
{
    public interface IPeriodEndValidateSubmissionWindowJobStatusService : IPeriodEndJobStatusService
    {
    }

    public class PeriodEndValidateSubmissionWindowJobStatusService : PeriodEndJobStatusService, IPeriodEndValidateSubmissionWindowJobStatusService
    {
        private readonly IMetricsClient metricsClient;

        public PeriodEndValidateSubmissionWindowJobStatusService(IMetricsClient metricsClient, IJobStorageService jobStorageService,
            IPaymentLogger logger, ITelemetry telemetry, IJobStatusEventPublisher eventPublisher,
            IJobServiceConfiguration config) : base(jobStorageService, logger, telemetry, eventPublisher, config)
        {
            this.metricsClient = metricsClient ?? throw new ArgumentNullException(nameof(metricsClient));
        }

        public override async Task<bool> ManageStatus(long jobId, CancellationToken cancellationToken)
        {
            Logger.LogVerbose($"Now determining if job {jobId} has finished.");
            var job = await JobStorageService.GetJob(jobId, cancellationToken).ConfigureAwait(false);
            
            Logger.LogDebug("Now using metrics client to determine if submissions window metrics are within tolerance.");
            var withinTolerance =
                await metricsClient.AreSubmissionWindowMetricsValid(job.AcademicYear, job.CollectionPeriod);
            if (withinTolerance)
                Logger.LogInfo("Submission window metrics are within tolerance.");
            else
                Logger.LogWarning("Submission window metrics are not within tolerance.");
            
            return await CompleteJob(job, withinTolerance ? JobStatus.Completed : JobStatus.CompletedWithErrors,
                DateTimeOffset.UtcNow, cancellationToken).ConfigureAwait(false);
        }

        protected override Task<bool> IsJobTimedOut(JobModel job, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }
}