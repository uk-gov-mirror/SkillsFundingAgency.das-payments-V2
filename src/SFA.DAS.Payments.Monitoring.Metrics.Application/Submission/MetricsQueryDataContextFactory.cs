using System;
using SFA.DAS.Payments.Monitoring.Metrics.Data;

namespace SFA.DAS.Payments.Monitoring.Metrics.Application.Submission
{
    public interface IMetricsQueryDataContextFactory
    {
        IMetricsQueryDataContext Create();
    }
    public class MetricsQueryDataContextFactory: IMetricsQueryDataContextFactory
    {
        private readonly Func<IMetricsQueryDataContext> dataContextFunc;

        public MetricsQueryDataContextFactory(Func<IMetricsQueryDataContext> dataContextFunc)
        {
            this.dataContextFunc = dataContextFunc ?? throw new ArgumentNullException(nameof(dataContextFunc));
        }

        public IMetricsQueryDataContext Create()
        {
            var queryDataContext = dataContextFunc();
            queryDataContext.SetTimeout(TimeSpan.FromSeconds(270));//TODO: use config
            return queryDataContext;
        }
    }
}