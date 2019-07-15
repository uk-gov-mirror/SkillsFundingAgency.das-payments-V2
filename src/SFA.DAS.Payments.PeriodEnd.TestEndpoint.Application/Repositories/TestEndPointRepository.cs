using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Remotion.Linq.Clauses;
using SFA.DAS.Payments.Monitoring.Jobs.Data;
using SFA.DAS.Payments.Monitoring.Jobs.Data.Model;

namespace SFA.DAS.Payments.PeriodEnd.TestEndpoint.Application.Repositories
{
    public class TestEndPointRepository : ITestEndPointRepository
    {

        private readonly IPaymentsDataContext paymentsDataContext;
        private readonly IJobsDataContext jobsDataContext;

        public TestEndPointRepository(IPaymentsDataContext paymentsDataContext, IJobsDataContext jobsDataContext)
        {
            this.paymentsDataContext = paymentsDataContext;
            this.jobsDataContext = jobsDataContext;
        }

        public async Task<List<SubmittedLearnerAimModel>> GetProviderLearnerAims(long ukprn, CancellationToken cancellationToken = default(CancellationToken))
        {
            var aims = await paymentsDataContext
                .SubmittedLearnerAim
                .Where(x => x.Ukprn == ukprn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return aims;
        }

        public async Task<List<long>> GetAccountIds(long ukprn, CancellationToken cancellationToken = default(CancellationToken))
        {
            var accountIdTuples = await paymentsDataContext
                .Apprenticeship
                .Where(x => x.Ukprn == ukprn)
                .Select(x => new { x.AccountId, x.TransferSendingEmployerAccountId })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var accountIds = accountIdTuples.Select(x => x.AccountId).ToList();
            var transferAccountIds = accountIdTuples
                                    .Where(o => o.TransferSendingEmployerAccountId.HasValue)
                                    .Select(o => o.TransferSendingEmployerAccountId.Value)
                                    .ToList();
            accountIds.AddRange(transferAccountIds);

            return accountIds.Distinct().ToList();
        }

        public async Task<List<long>> GetUkprns(CancellationToken cancellationToken = default(CancellationToken))
        {
            var ukprns = await paymentsDataContext
                .Apprenticeship
                .Select(x => x.Ukprn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);


            return ukprns.Distinct().ToList();
        }

        public async Task CreateMonitoringJob(JobModel job, List<JobStepModel> jobSteps, CancellationToken cancellationToken = default(CancellationToken))
        {
            await jobsDataContext.SaveNewJob(job, jobSteps, cancellationToken).ConfigureAwait(false);
        }

    }
}