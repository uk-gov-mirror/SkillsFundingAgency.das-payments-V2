﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using SFA.DAS.Payments.Messages.Core;
using SFA.DAS.Payments.Monitoring.Jobs.Messages;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Monitoring.Jobs.Client.Infrastructure.Messaging
{
    public class JobStatusOutgoingMessageBehaviour : Behavior<IOutgoingLogicalMessageContext>
    {
        public override async Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            if (context.Message.Instance is IPaymentsMessage && !(context.Message.Instance is JobsMessage) && context.Extensions.TryGet(JobStatusBehaviourConstants.GeneratedMessagesKey, out List<GeneratedMessage> generatedMessages))
                generatedMessages.Add(new GeneratedMessage
                {
                    StartTime = DateTimeOffset.UtcNow,
                    MessageId = context.GetMessageId(),
                    MessageName = context.GetMessageName()
                });
        }
    }
}