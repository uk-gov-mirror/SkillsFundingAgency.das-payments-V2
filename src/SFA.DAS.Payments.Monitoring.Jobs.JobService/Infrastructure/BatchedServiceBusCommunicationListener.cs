using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Autofac;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Ioc;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.UnitOfWork;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.Monitoring.Jobs.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Application.JobMessageProcessing;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Monitoring.Jobs.JobService.Infrastructure
{
    public interface IBatchedServiceBusCommunicationListener : ICommunicationListener
    {
        string EndpointName { get; set; }
    }


    public class BatchedServiceBusCommunicationListener : IBatchedServiceBusCommunicationListener
    {
        private readonly IApplicationConfiguration config;
        private readonly IPaymentLogger logger;
        private readonly IContainerScopeFactory scopeFactory;
        private readonly string connectionString;
        public string EndpointName { get; set; }
        private readonly string errorQueueName;
        private CancellationToken startingCancellationToken;
        private Task listenForMessagesThread;

        public BatchedServiceBusCommunicationListener(IApplicationConfiguration config, IPaymentLogger logger,
            IContainerScopeFactory scopeFactory)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            this.connectionString = config.ServiceBusConnectionString;
            EndpointName = config.EndpointName;
            this.errorQueueName = config.FailedMessagesQueue;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            startingCancellationToken = cancellationToken;
            listenForMessagesThread = ListenForMessages(cancellationToken);
            return EndpointName;
        }

        protected virtual async Task ListenForMessages(CancellationToken cancellationToken)
        {
            await EnsureQueue(EndpointName).ConfigureAwait(false);
            var connection = new ServiceBusConnection(connectionString);
            try
            {
                await Task.WhenAll(
                    Listen(connection, cancellationToken),
                    Listen(connection, cancellationToken));

            }
            catch (Exception ex)
            {
                logger.LogFatal($"Encountered fatal error. Error: {ex.Message}", ex);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        private async Task Listen(ServiceBusConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var messageReceiver = new MessageReceiver(connection, EndpointName, ReceiveMode.PeekLock,
                        RetryPolicy.Default, 0);
                    var messages = new List<Message>();
                    for (int i = 0; i < 3; i++)
                    {
                        var receivedMessages = await messageReceiver.ReceiveAsync(200, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        if (receivedMessages == null || !receivedMessages.Any())
                            break;
                        messages.AddRange(receivedMessages);
                    }
                    if (!messages.Any())
                    {
                        await Task.Delay(2000, cancellationToken);
                        continue;
                    }

                    var jobStatuses = messages.Where(IsJobStatusMessage)
                        .ToList();

                    var tasks = new List<Task>() { ProcessMessages(jobStatuses, messageReceiver, cancellationToken) };
                    tasks.AddRange(messages.Where(msg => !IsJobStatusMessage(msg)).Select(msg => ProcessMessage(msg, messageReceiver, cancellationToken)));
                    await Task.WhenAll(tasks);

                    await messageReceiver.CompleteAsync(messages.Select(message =>
                        message.SystemProperties.LockToken));
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error listening for message.  Error: {ex.Message}", ex);
                throw;
            }
        }

        private bool IsJobStatusMessage(Message receivedMessage)
        {
            if (!receivedMessage.UserProperties.ContainsKey(NServiceBus.Headers.EnclosedMessageTypes))
                return false;

            var enclosedTypes = (string)receivedMessage.UserProperties[NServiceBus.Headers.EnclosedMessageTypes];
            return enclosedTypes.Contains(
                "SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands.RecordJobMessageProcessingStatus",
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task ProcessMessage(Message receivedMessage, MessageReceiver messageReceiver,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!receivedMessage.UserProperties.ContainsKey(NServiceBus.Headers.EnclosedMessageTypes))
                {
                    return;
                }

                var enclosedTypes = (string)receivedMessage.UserProperties[NServiceBus.Headers.EnclosedMessageTypes];
                var typeName = enclosedTypes.Split(';').FirstOrDefault();
                if (string.IsNullOrEmpty(typeName))
                    return;
                var messageType = Type.GetType(typeName);
                var sanitisedMessageJson = GetMessagePayload(receivedMessage);
                var monitoringMessage = JsonConvert.DeserializeObject(sanitisedMessageJson, messageType);
                using (var scope = scopeFactory.CreateScope())
                {
                    var handler = scope.Resolve(typeof(IHandleMessages<>).MakeGenericType(messageType));
                    var methodInfo = handler.GetType().GetMethod("Handle");
                    if (methodInfo == null)
                        throw new InvalidOperationException($"Handle method not found on NSB handler: {handler.GetType().Name} for message type: {typeName}");
                    await (Task)methodInfo.Invoke(handler, new object[] { monitoringMessage, null });
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error processing message. Error: {e.Message}", e);
                //await messageReceiver.AbandonAsync(receivedMessage.SystemProperties.LockToken);
                throw;
            }

        }

        private async Task ProcessMessages(List<Message> receivedMessages, MessageReceiver messageReceiver,
            CancellationToken cancellationToken)
        {
            try
            {
                var recordJobStatusMessages = receivedMessages
                    .Select(msg =>
                        JsonConvert.DeserializeObject<RecordJobMessageProcessingStatus>(GetMessagePayload(msg)))
                    .ToList();

                using (var containerScope = scopeFactory.CreateScope())
                {
                    var unitOfWorkScopeFactory = containerScope.Resolve<IUnitOfWorkScopeFactory>();
                    using (var scope = unitOfWorkScopeFactory.Create($"JobService.RecordJobMessageProcessingStatus"))
                    {
                        var service = scope.Resolve<IJobMessageService>();
                        await service.RecordCompletedJobMessageStatus(recordJobStatusMessages, cancellationToken);
                        await scope.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error processing message. Error: {e.Message}", e);
                //await messageReceiver.AbandonAsync(receivedMessage.SystemProperties.LockToken);
                throw;
            }

        }

        private string GetMessagePayload(Message receivedMessage)
        {
            var doc = receivedMessage.GetBody<XmlElement>();
            var messageBody = Convert.FromBase64String(doc.InnerText);
            var monitoringMessageJson = Encoding.UTF8.GetString(messageBody);
            var sanitisedMessageJson = monitoringMessageJson
                .Trim(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())
                    .ToCharArray());
            return sanitisedMessageJson;
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            if (!startingCancellationToken.IsCancellationRequested)
                startingCancellationToken = cancellationToken;
        }

        public void Abort()
        {
            //
        }

        private async Task EnsureQueue(string queuePath)
        {
            try
            {
                var manageClient = new ManagementClient(connectionString);
                if (await manageClient.QueueExistsAsync(queuePath, startingCancellationToken).ConfigureAwait(false))
                    return;

                var queueDescription = new QueueDescription(queuePath)
                {
                    DefaultMessageTimeToLive = TimeSpan.FromMinutes(30),
                    EnableDeadLetteringOnMessageExpiration = true,
                    LockDuration = TimeSpan.FromMinutes(1),
                    MaxDeliveryCount = 10,
                    MaxSizeInMB = 5120,
                    Path = queuePath
                };

                await manageClient.CreateQueueAsync(queueDescription, startingCancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error ensuring queue: {e.Message}.");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}