using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Messages.Core;
using SFA.DAS.Payments.Messages.Core.Events;

namespace SFA.DAS.Payments.Application.Messaging.Telemetry
{
    public class TelemetryHandlerBehaviour :
        Behavior<IInvokeHandlerContext>
    {
        private readonly ITelemetry telemetry;

        public TelemetryHandlerBehaviour(ITelemetry telemetry)
        {
            this.telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            var operationId = (context.MessageBeingHandled as IEvent)?.EventId.ToString();
            using (var operation = telemetry.StartOperation(context.MessageHandler.HandlerType.FullName, operationId))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var failure = string.Empty;
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    failure = ex.Message;
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                    var properties = new Dictionary<string,string>();
                    if (string.IsNullOrEmpty(failure))
                    {
                        properties.Add("Failed", "True");
                        properties.Add("Failure", failure);
                    }
                    else 
                        properties.Add("Failed", "False");

                    if (context.MessageBeingHandled is IJobMessage jobMessage)
                    {
                        properties.Add(TelemetryKeys.JobId, jobMessage.JobId.ToString());
                    }
                    
                    telemetry.TrackEvent(context.MessageHandler.HandlerType.FullName,properties,
                        new Dictionary<string, double> { { TelemetryKeys.Duration, stopwatch.ElapsedMilliseconds } });
                    telemetry.StopOperation(operation);
                }
            }
        }
    }
}