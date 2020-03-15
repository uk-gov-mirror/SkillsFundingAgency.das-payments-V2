using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.FundingSource.Application.LevyTransactionStorage
{
    public interface ILevyTransactionBatchStorageService
    {
        Task StoreLevyTransactions(IList<CalculatedRequiredLevyAmount> levyTransactions);
    }

    public class LevyTransactionBatchStorageService : ILevyTransactionBatchStorageService
    {
        private readonly IPaymentLogger logger;

        public LevyTransactionBatchStorageService(IPaymentLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StoreLevyTransactions(IList<CalculatedRequiredLevyAmount> levyTransactions)
        {
            logger.LogDebug($"Got {levyTransactions.Count} levy transactions.");
        }
    }
}