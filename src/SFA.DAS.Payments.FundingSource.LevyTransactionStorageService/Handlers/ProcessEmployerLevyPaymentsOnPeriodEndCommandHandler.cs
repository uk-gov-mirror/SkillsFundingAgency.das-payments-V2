using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.FundingSource.LevyFundedService.Interfaces;
using SFA.DAS.Payments.FundingSource.LevyTransactionStorageService.Infrastructure.Messaging;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using SFA.DAS.Payments.FundingSource.Messages.Internal.Commands;

namespace SFA.DAS.Payments.FundingSource.LevyTransactionStorageService.Handlers
{
    public class ProcessEmployerLevyPaymentsOnPeriodEndCommandHandler : IHandleMessageBatches<ProcessLevyPaymentsOnMonthEndCommand>
    {
        private readonly ITelemetry telemetry;
        private readonly IActorProxyFactory proxyFactory;
        private readonly IPaymentLogger logger;
        private readonly IMessageSessionFactory factory;

        public ProcessEmployerLevyPaymentsOnPeriodEndCommandHandler(ITelemetry telemetry, IActorProxyFactory proxyFactory, IPaymentLogger logger, IMessageSessionFactory factory)
        {
            this.telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            this.proxyFactory = proxyFactory ?? throw new ArgumentNullException(nameof(proxyFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task Handle(ProcessLevyPaymentsOnMonthEndCommand command, IMessageSession context, CancellationToken cancellationToken)
        {
            logger.LogDebug($"Processing employer levy payments. Employer: {command.AccountId}, Job: {command.JobId}");
            try
            {
                using (var operation = telemetry.StartOperation("LevyTransactionStorageService.ProcessLevyPaymentsOnMonthEndCommand", command.AccountId.ToString()))
                {
                    var stopwatch = Stopwatch.StartNew();
                    var actorId = new ActorId(command.AccountId.ToString());
                    var actor = proxyFactory.CreateActorProxy<ILevyFundedService>(new Uri("fabric:/SFA.DAS.Payments.FundingSource.ServiceFabric/LevyFundedServiceActorService"), actorId);
                    var fundingSourceEvents = await actor.HandleMonthEnd(command).ConfigureAwait(false);
                    foreach (var fundingSourcePaymentEvent in fundingSourceEvents)
                    {
                        if (fundingSourcePaymentEvent is ProcessUnableToFundTransferFundingSourcePayment)
                            await context.SendLocal(fundingSourcePaymentEvent).ConfigureAwait(false);
                        else
                            await context.Publish(fundingSourcePaymentEvent).ConfigureAwait(false);
                    }

                    telemetry.TrackDurationWithMetrics("LevyFundedProxyService.ProcessLevyPaymentsOnMonthEndCommand",
                        stopwatch,
                        command,
                        command.AccountId,
                        new Dictionary<string, double>
                        {
                            { TelemetryKeys.Count, fundingSourceEvents.Count }
                        });

                    telemetry.StopOperation(operation);
                }

            }
            catch (Exception e)
            {
                logger.LogFatal($"Failed to process month end for employer: {command.AccountId}. Error: {e.Message}", e);
            }
        }

        public async Task Handle(IList<ProcessLevyPaymentsOnMonthEndCommand> messages, CancellationToken cancellationToken)
        {
            logger.LogDebug($"Generating period end levy payments for employers: {messages.Aggregate(string.Empty, (s, msg) => $"{s}, {msg.AccountId}")}");
            var messageSession = factory.Create();
            await Task.WhenAll(messages.Select(message => Handle(message, messageSession, cancellationToken)))
                .ConfigureAwait(false);

            logger.LogInfo($"Finished generating period end levy payments for employers: {messages.Aggregate(string.Empty, (s, msg) => $"{s}, {msg.AccountId}")}");
        }
    }
}