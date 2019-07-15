using SFA.DAS.Payments.Model.Core.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Monitoring.Jobs.Data.Model;

namespace SFA.DAS.Payments.PeriodEnd.TestEndpoint.Application.Repositories
{
    public interface ITestEndPointRepository
    {
        Task<List<SubmittedLearnerAimModel>> GetProviderLearnerAims(long ukprn, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<long>> GetAccountIds(long ukprn, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<long>> GetUkprns(CancellationToken cancellationToken = default(CancellationToken));
        Task CreateMonitoringJob(JobModel job, CancellationToken cancellationToken = default(CancellationToken));
    }
}
