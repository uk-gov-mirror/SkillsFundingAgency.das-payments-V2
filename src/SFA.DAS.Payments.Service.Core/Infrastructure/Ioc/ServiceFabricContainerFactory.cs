﻿using Autofac;
using Autofac.Integration.ServiceFabric;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SFA.DAS.Payments.Application.Infrastructure.Ioc;

namespace SFA.DAS.Payments.ServiceFabric.Core.Infrastructure.Ioc
{
    public class ServiceFabricContainerFactory
    {
        public static ContainerBuilder CreateBuilderForActor<TActor>() where TActor: ActorBase
        {
            var builder = ContainerFactory.CreateBuilder();
            builder.RegisterActor<TActor>();
            return builder;
        }

        public static ContainerBuilder CreateBuilderForStatelessService<TStatelessService>() where TStatelessService: StatelessService
        {
            return CreateBuilderForStatelessService<TStatelessService>(typeof(TStatelessService).FullName + "Type");
        }

        public static ContainerBuilder CreateBuilderForStatelessService<TStatelessService>(string serviceTypeName) where TStatelessService: StatelessService
        {
            var builder = ContainerFactory.CreateBuilder();
            builder.RegisterStatelessService<TStatelessService>("SFA.DAS.Payments.RequiredPayments.RequiredPaymentsProxyServiceType");
            return builder;
        }
    }
}