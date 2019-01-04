﻿using Autofac;
using ESFA.DC.Logging.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.RequiredPayments.Domain;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.RequiredPaymentsService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.RequiredPayments.RequiredPaymentsProxyService.Handlers
{
    public class EarningEventHandler : IHandleMessages<EarningEvent>
    {
        private readonly IApprenticeshipKeyService apprenticeshipKeyService;
        private readonly IActorProxyFactory proxyFactory;
        private readonly IPaymentLogger paymentLogger;
        private readonly ILifetimeScope lifetimeScope;

        public EarningEventHandler(IApprenticeshipKeyService apprenticeshipKeyService,
            IActorProxyFactory proxyFactory,
            IPaymentLogger paymentLogger,
            ILifetimeScope lifetimeScope)
        {
            this.apprenticeshipKeyService = apprenticeshipKeyService;
            this.proxyFactory = proxyFactory ?? new ActorProxyFactory();
            this.paymentLogger = paymentLogger;
            this.lifetimeScope = lifetimeScope;
        }

        public async Task Handle(EarningEvent message, IMessageHandlerContext context)
        {
            paymentLogger.LogInfo($"Processing RequiredPaymentsProxyService event. Message Id : {context.MessageId}");

            var executionContext = (ESFA.DC.Logging.ExecutionContext)lifetimeScope.Resolve<IExecutionContext>();
            executionContext.JobId = message.JobId.ToString();

            try
            {
                var key = apprenticeshipKeyService.GenerateApprenticeshipKey(
                    message.Ukprn,
                    message.Learner.ReferenceNumber,
                    message.LearningAim.FrameworkCode,
                    message.LearningAim.PathwayCode,
                    message.LearningAim.ProgrammeType,
                    message.LearningAim.StandardCode,
                    message.LearningAim.Reference
                );

                var actorId = new ActorId(key);
                var actor = proxyFactory.CreateActorProxy<IRequiredPaymentsService>(new Uri("fabric:/SFA.DAS.Payments.RequiredPayments.ServiceFabric/RequiredPaymentsServiceActorService"), actorId);
                IReadOnlyCollection<RequiredPaymentEvent> requiredPaymentEvent;
                try
                {
                    requiredPaymentEvent = await actor.HandleEarningEvent(message, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    paymentLogger.LogError($"Error invoking required payments actor. Error: {ex.Message}", ex);
                    throw;
                }

                try
                {
                    if (requiredPaymentEvent != null)
                        await Task.WhenAll(requiredPaymentEvent.Select(context.Publish)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    //TODO: add more details when we flesh out the event.
                    paymentLogger.LogError($"Error publishing the event: 'RequiredPaymentEvent'.  Error: {ex.Message}.", ex);
                    throw;
                    //TODO: update the job
                }

                paymentLogger.LogInfo($"Successfully processed RequiredPaymentsProxyService event for Actor Id {actorId}");
            }
            catch (Exception ex)
            {
                paymentLogger.LogError("Error while handling RequiredPaymentsProxyService event", ex);
                throw;
            }
        }
    }
}