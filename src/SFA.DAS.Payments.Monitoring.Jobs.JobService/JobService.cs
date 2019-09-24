using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.UnitOfWork;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Monitoring.Jobs.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Application.JobProcessing;
using SFA.DAS.Payments.Monitoring.Jobs.JobService.Infrastructure;
using SFA.DAS.Payments.Monitoring.Jobs.JobService.Interfaces;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;
using SFA.DAS.Payments.Monitoring.Jobs.Model;
using SFA.DAS.Payments.ServiceFabric.Core;
using SFA.DAS.Payments.ServiceFabric.Core.Infrastructure.Ioc;

namespace SFA.DAS.Payments.Monitoring.Jobs.JobService
{
    [StatePersistence(StatePersistence.Volatile)]
    public class JobService : StatefulService, IJobService, IService
    {
        private readonly IPaymentLogger logger;
        private readonly IUnitOfWorkScopeFactory scopeFactory;
        private readonly IJobStatusManager jobStatusManager;
        private readonly ILifetimeScope lifetimeScope;
        //private IStatefulEndpointCommunicationListener listener;

        public JobService(StatefulServiceContext context, IPaymentLogger logger, IUnitOfWorkScopeFactory scopeFactory, IJobStatusManager jobStatusManager, ILifetimeScope lifetimeScope)
            : base(context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            this.jobStatusManager = jobStatusManager ?? throw new ArgumentNullException(nameof(jobStatusManager));
            this.lifetimeScope = lifetimeScope;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            try
            {
                EndpointConfigurationEvents.ConfiguringEndpointName += (object sender, Payments.Application.Infrastructure.Ioc.Modules.EndpointName e) =>
                {
                    e.Name += ((NamedPartitionInformation)Partition.PartitionInfo).Name;
                };
                //var serviceListener = new ServiceReplicaListener(context =>
//                    listener = lifetimeScope.Resolve<IStatefulEndpointCommunicationListener>());

                var batchListener = lifetimeScope.Resolve<IBatchedServiceBusCommunicationListener>();
                batchListener.EndpointName += ((NamedPartitionInformation)Partition.PartitionInfo).Name;
                var serviceListener = new ServiceReplicaListener(context => batchListener);
                return new List<ServiceReplicaListener>
                {
                    serviceListener,
                };

            }
            catch (Exception e)
            {
                logger.LogError($"Error: {e.Message} ",e);
                throw;
            }
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            var endpoint = lifetimeScope.Resolve<EndpointConfiguration>();
            return Task.WhenAll(Endpoint.Create(endpoint), jobStatusManager.Start(cancellationToken));

//            return Task.WhenAll(listener.RunAsync(), jobStatusManager.Start(cancellationToken));
        }


    }
}
