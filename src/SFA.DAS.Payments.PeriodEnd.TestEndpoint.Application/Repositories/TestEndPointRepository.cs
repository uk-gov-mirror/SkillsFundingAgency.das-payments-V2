using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remotion.Linq.Clauses;

namespace SFA.DAS.Payments.PeriodEnd.TestEndpoint.Application.Repositories
{
    public class TestEndPointRepository : ITestEndPointRepository
    {

        private readonly IPaymentsDataContext paymentsDataContext;

        public TestEndPointRepository(IPaymentsDataContext paymentsDataContext)
        {
            this.paymentsDataContext = paymentsDataContext;
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

        public async Task<List<long>> GetAccountIds( long ukprn,CancellationToken cancellationToken = default(CancellationToken))
        {
            var accountIdTuples = await paymentsDataContext
                .Apprenticeship
                .Where(x => x.Ukprn == ukprn)
                .Select(x => new {x.AccountId, x.TransferSendingEmployerAccountId})
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

        public async Task<List<SubmittedLearnerAimModel>> CreateMonitoringJob(long ukprn, CancellationToken cancellationToken = default(CancellationToken))
        {
            var aims = await paymentsDataContext
                .J
                .Where(x => x.Ukprn == ukprn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return aims;
        }

    }
}