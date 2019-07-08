using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<List<long>> GetAccountIds( CancellationToken cancellationToken = default(CancellationToken))
        {
            var accountIds = await paymentsDataContext
                .Apprenticeship
                .Select(x => x.AccountId)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return accountIds;
        }

        public async Task<List<long>> GetTransferAccountIds(CancellationToken cancellationToken = default(CancellationToken))
        {
            var accountIds = await paymentsDataContext
                .Apprenticeship
                .Where(x => x.TransferSendingEmployerAccountId.HasValue)
                .Select(x => x.TransferSendingEmployerAccountId.GetValueOrDefault())
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return accountIds;
        }

    }
}