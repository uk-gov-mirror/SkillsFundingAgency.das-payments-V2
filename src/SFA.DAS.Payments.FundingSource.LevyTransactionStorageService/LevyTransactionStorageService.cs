using System;
using System.Collections.Generic;
using System.Fabric;
using Autofac;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.ServiceFabric.Core;

namespace SFA.DAS.Payments.FundingSource.LevyTransactionStorageService
{
    public class LevyTransactionStorageService : StatelessService
    {
        private readonly IPaymentLogger logger;
        private readonly ILifetimeScope lifetimeScope;

        public LevyTransactionStorageService(StatelessServiceContext context, IPaymentLogger logger, ILifetimeScope lifetimeScope)
            : base(context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            try
            {
                return new List<ServiceInstanceListener>
                {
                    new ServiceInstanceListener(context => lifetimeScope.Resolve<IStatelessServiceBusBatchCommunicationListener>())
            };
            }
            catch (Exception e)
            {
                logger.LogError($"Error starting the service instance listener: {e.Message}", e);
                throw;
            }
        }
    }
}
