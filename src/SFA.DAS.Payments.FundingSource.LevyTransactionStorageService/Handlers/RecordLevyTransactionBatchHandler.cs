using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.FundingSource.LevyTransactionStorageService.Handlers
{
    public class RecordLevyTransactionBatchHandler : IHandleMessageBatches<CalculatedRequiredLevyAmount>
    {
        private readonly IPaymentLogger logger;

        public RecordLevyTransactionBatchHandler(IPaymentLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(IList<CalculatedRequiredLevyAmount> messages, CancellationToken cancellationToken)
        {
            logger.LogInfo($"Received {messages.Count} messages");
        }
    }
}