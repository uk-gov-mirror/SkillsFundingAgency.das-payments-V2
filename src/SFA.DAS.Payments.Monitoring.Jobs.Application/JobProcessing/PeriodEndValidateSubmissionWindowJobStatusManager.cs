using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.UnitOfWork;
using SFA.DAS.Payments.Monitoring.Jobs.Application.Infrastructure.Configuration;
using SFA.DAS.Payments.Monitoring.Jobs.Application.JobProcessing.PeriodEnd;

namespace SFA.DAS.Payments.Monitoring.Jobs.Application.JobProcessing
{
    public interface IPeriodEndValidateSubmissionWindowJobStatusManager : IPeriodEndJobStatusManager { }

    public class PeriodEndValidateSubmissionWindowJobStatusManager : PeriodEndJobStatusManager, IPeriodEndValidateSubmissionWindowJobStatusManager
    {
        public PeriodEndValidateSubmissionWindowJobStatusManager(IPaymentLogger logger, IUnitOfWorkScopeFactory scopeFactory,
            IJobServiceConfiguration configuration) : base(logger, scopeFactory, configuration)
        {
        }

        public override IJobStatusService GetJobStatusService(IUnitOfWorkScope scope)
        {
            return scope.Resolve<IPeriodEndValidateSubmissionWindowJobStatusService>();
        }

        public override async Task<List<long>> GetCurrentJobs(IJobStorageService jobStorage)
        {
            return await jobStorage.GetCurrentPeriodEndValidateSubmissionWindowJobs(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}