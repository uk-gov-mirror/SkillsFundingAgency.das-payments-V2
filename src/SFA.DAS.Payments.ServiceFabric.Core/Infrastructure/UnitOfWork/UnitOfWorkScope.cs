using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Infrastructure.UnitOfWork;
using SFA.DAS.Payments.ServiceFabric.Core.Batch;

namespace SFA.DAS.Payments.ServiceFabric.Core.Infrastructure.UnitOfWork
{
    public class UnitOfWorkScope : IUnitOfWorkScope
    {
        private readonly string operationName;
        protected ILifetimeScope LifetimeScope { get; }
        private readonly IReliableStateManagerTransactionProvider transactionProvider;
        private readonly ITelemetry telemetry;
        private readonly IOperationHolder<RequestTelemetry> operation;
        private readonly Stopwatch stopwatch;

        public UnitOfWorkScope(ILifetimeScope lifetimeScope, string operationName)
        {
            this.operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            this.LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
            var stateManager = lifetimeScope.Resolve<IReliableStateManagerProvider>().Current;
            transactionProvider = lifetimeScope.Resolve<IReliableStateManagerTransactionProvider>();
            ((ReliableStateManagerTransactionProvider)transactionProvider).Current = stateManager.CreateTransaction();
            telemetry = lifetimeScope.Resolve<ITelemetry>();
            operation = telemetry.StartOperation(operationName);
            stopwatch = Stopwatch.StartNew();
        }

        public T Resolve<T>()
        {
            return LifetimeScope.Resolve<T>();
        }

        public void Dispose()
        {
            stopwatch.Restart();
            telemetry?.StopOperation(operation);
            operation?.Dispose();
            transactionProvider.Current.Dispose();
            ((ReliableStateManagerTransactionProvider)transactionProvider).Current = null;
            telemetry.TrackDuration($"{operationName}_DisposeScope",stopwatch.Elapsed);
            LifetimeScope?.Dispose();
        }

        public void Abort()
        {
            transactionProvider.Current.Abort();
        }

        public async Task Commit()
        {
            telemetry.TrackDuration(operationName, stopwatch.Elapsed);
            stopwatch.Restart();
            await transactionProvider.Current.CommitAsync();
            telemetry.TrackDuration($"{operationName}_CommitTransaction", stopwatch.Elapsed);
        }
    }
}