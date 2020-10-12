using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Monitoring.Jobs.Application.JobProcessing.PeriodEnd
{
    public interface IMetricsClient
    {
        Task<bool> AreSubmissionWindowMetricsValid(int academicYear, int collectionPeriod);
    }

    public class MetricsClient: IMetricsClient
    {
        private readonly string metricsEndpoint;
        private HttpClient httpClient;

        public MetricsClient(string metricsEndpoint)
        {
            this.metricsEndpoint = metricsEndpoint;
            httpClient = new HttpClient{BaseAddress = new Uri(metricsEndpoint)};
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> AreSubmissionWindowMetricsValid(int academicYear, int collectionPeriod)
        {
            var response = await httpClient.GetAsync($"?academicYear={academicYear}&collectionPeriod={collectionPeriod}");
            return response.IsSuccessStatusCode;
        }
    }
}