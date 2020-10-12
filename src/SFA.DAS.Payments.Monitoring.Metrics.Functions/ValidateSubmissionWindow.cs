using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Core;
using SFA.DAS.Payments.Monitoring.Metrics.Application.Submission;

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
                var collectionPeriod = byte.Parse(req.Query["collectionPeriod"]);
                var academicYear = short.Parse(req.Query["academicYear"]);
                var result = await service.ValidateSubmissionWindow(academicYear, collectionPeriod);
                return result.IsValid 
                    ? (IActionResult)new OkObjectResult(result.SubmissionsSummary.ToJson())
                    : new BadRequestResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error: {ex.Message}. {ex}");
                return new InternalServerErrorResult();
            }
        }
    }
}