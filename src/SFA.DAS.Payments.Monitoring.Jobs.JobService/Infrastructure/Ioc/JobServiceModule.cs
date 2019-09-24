using Autofac;
using NServiceBus;
using SFA.DAS.Payments.Monitoring.Jobs.Application;
using SFA.DAS.Payments.Monitoring.Jobs.JobService.Handlers;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Monitoring.Jobs.JobService.Infrastructure.Ioc
{
    public class JobServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobStorageService>()
                .As<IJobStorageService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<BatchedServiceBusCommunicationListener>()
                .As<IBatchedServiceBusCommunicationListener>();

            builder.RegisterType<RecordEarningsJobHandler>()
                .As<IHandleMessages<RecordEarningsJob>>()
                .InstancePerLifetimeScope();

            builder.RegisterType<RecordEarningsJobAdditionalMessagesHandler>()
                .As<IHandleMessages<RecordEarningsJobAdditionalMessages>>()
                .InstancePerLifetimeScope();

            builder.RegisterType<RecordJobMessageProcessingStatusHandler>()
                .As<IHandleMessages<RecordJobMessageProcessingStatus>>()
                .InstancePerLifetimeScope();

        }
    }
}