using System;

namespace SFA.DAS.Payments.FundingSource.PerformanceTests
{

    public class ConnectionStrings
    {
        public string StorageConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string PaymentsConnectionString { get; set; }
    }

    public class AppSettings
    {
        public string TestsEndpointName { get; set; }
        public TimeSpan TimeToWait { get; set; }
        public string LevyEndPoint { get; set; }
        public string NonLevyEndpoint { get; set; }
    }



    public class Config
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public AppSettings AppSettings { get; set; }
    }
}