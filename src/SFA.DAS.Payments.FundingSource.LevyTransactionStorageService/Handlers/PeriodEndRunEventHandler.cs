using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.FundingSource.Application.Interfaces;
using SFA.DAS.Payments.FundingSource.LevyTransactionStorageService.Infrastructure.Messaging;
using SFA.DAS.Payments.PeriodEnd.Messages.Events;

namespace SFA.DAS.Payments.FundingSource.LevyTransactionStorageService.Handlers
{
    public class PeriodEndRunEventHandler: IHandleMessageBatches<PeriodEndRunningEvent>
    {
        private readonly IPaymentLogger logger;
        private readonly IPeriodEndService periodEndService;
        private readonly IMessageSessionFactory factory;

        public PeriodEndRunEventHandler(IPaymentLogger logger, IPeriodEndService periodEndService, IMessageSessionFactory factory)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.periodEndService = periodEndService ?? throw new ArgumentNullException(nameof(periodEndService));
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task Handle(IList<PeriodEndRunningEvent> messages, CancellationToken cancellationToken)
        {
            //TODO: Temp code, does not deal with rollover scenarios
            var message = messages.FirstOrDefault();
            if (message == null)
                throw new InvalidOperationException("No valid period end running events found.");
            var employerPeriodEndCommands = await periodEndService.GenerateEmployerPeriodEndCommands(message).ConfigureAwait(false);
            logger.LogDebug($"Got {employerPeriodEndCommands.Count} employer period end commands.");
            var messageSession = factory.Create();
            foreach (var processLevyPaymentsOnMonthEndCommand in employerPeriodEndCommands)
            {
                logger.LogDebug($"Sending period end command for employer '{processLevyPaymentsOnMonthEndCommand.AccountId}'");
                await messageSession.SendLocal(processLevyPaymentsOnMonthEndCommand).ConfigureAwait(false);
                logger.LogDebug($"Sent period end command for employer '{processLevyPaymentsOnMonthEndCommand.AccountId}'");
            }
            logger.LogInfo($"Finished sending employer period end commands for collection: {message.CollectionPeriod.Period:00}-{message.CollectionPeriod.AcademicYear:0000}.");
        }
    }
}