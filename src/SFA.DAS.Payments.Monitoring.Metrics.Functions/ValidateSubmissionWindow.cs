using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Core;
using SFA.DAS.Payments.Monitoring.Metrics.Application.Submission;
using SFA.DAS.Payments.Monitoring.Metrics.Data;

namespace SFA.DAS.Payments.Monitoring.Metrics.Functions
{
    public class ValidateSubmissionWindow
    {
        private readonly ISubmissionWindowSummaryService service;

        public ValidateSubmissionWindow(ISubmissionWindowSummaryService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [FunctionName("ValidateSubmissionWindow")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context, 
            ILogger log)
        {
            try
            {
                var result = await service.ValidateSubmissionWindow(2021, 2);
                return result.Item1 
                    ? (IActionResult)new OkObjectResult(result.Item2.ToJson())
                    : new BadRequestResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error: {ex.Message}. {ex}");
                return new InternalServerErrorResult();
            }
        }


        //[FunctionName("ValidateSubmissionWindow")]
        //public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        //    ExecutionContext context, ILogger log)
        //{
        //    try
        //    {
        //        var config = new ConfigurationBuilder()
        //            .SetBasePath(context.FunctionAppDirectory)
        //            .AddJsonFile("local.settings.json")
        //            .AddEnvironmentVariables()
        //            .Build();

        //        var cnn = config["PaymentsConnectionString"];

        //        var dataContext = new MetricsPersistenceDataContext(cnn);

        //        var submissionSummary = await dataContext.GetSubmissionsSummary(2021, 2);

        //        return submissionSummary.Percentage > 99.3M
        //            ? (IActionResult) new OkObjectResult(submissionSummary.ToJson())
        //            : new BadRequestResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError($"Error: {ex.Message}. {ex}");
        //        return new InternalServerErrorResult();
        //    }
        //}
    }
}