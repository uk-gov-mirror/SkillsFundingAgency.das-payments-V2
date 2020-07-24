using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.Payments.Monitoring.Metrics.Data;

namespace SFA.DAS.Payments.Monitoring.Metrics.Application.Submission
{
    public interface IMetricsQueryTransactionFactory
    {
        Task<IDbContextTransaction> Create(CancellationToken cancellationToken);

        Task<IDbContextTransaction> Create(IsolationLevel isolationLevel,
            CancellationToken cancellationToken);
    }

    public class MetricsQueryTransactionFactory : IMetricsQueryTransactionFactory
    {
        private readonly IMetricsQueryDataContext queryDataContext;

        public MetricsQueryTransactionFactory(IMetricsQueryDataContext queryDataContext)
        {
            this.queryDataContext = queryDataContext ?? throw new ArgumentNullException(nameof(queryDataContext));
        }

        public async Task<IDbContextTransaction> Create(CancellationToken cancellationToken)
        {
            return await queryDataContext.BeginTransaction(IsolationLevel.ReadUncommitted, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IDbContextTransaction> Create(IsolationLevel isolationLevel,
            CancellationToken cancellationToken)
        {
            return await queryDataContext.BeginTransaction(isolationLevel, cancellationToken).ConfigureAwait(false);
        }
    }
}