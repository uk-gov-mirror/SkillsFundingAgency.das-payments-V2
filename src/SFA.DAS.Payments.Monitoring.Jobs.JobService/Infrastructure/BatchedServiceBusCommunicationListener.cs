using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Infrastructure.UnitOfWork;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.Monitoring.Jobs.Application;
using SFA.DAS.Payments.Monitoring.Jobs.Application.JobMessageProcessing;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;
using SFA.DAS.Payments.ServiceFabric.Core.Infrastructure.UnitOfWork;

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
        private readonly ITelemetry telemetry;
        private readonly string connectionString;
        public string EndpointName { get; set; }
        private readonly string errorQueueName;
        private CancellationToken startingCancellationToken;
        private Task listenForMessagesThread;

        public BatchedServiceBusCommunicationListener(IApplicationConfiguration config, IPaymentLogger logger,
            IContainerScopeFactory scopeFactory, ITelemetry telemetry)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            this.telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
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
            //var connection = new ServiceBusConnection(connectionString);
            try
            {
                await Task.WhenAll(
                    //                    Listen(cancellationToken),
                                        Listen(cancellationToken),
                    Listen(cancellationToken)
                    //                Listen(connection, cancellationToken)
                    );
            }
            catch (Exception ex)
            {
                logger.LogFatal($"Encountered fatal error. Error: {ex.Message}", ex);
            }
            finally
            {
            //    await connection.CloseAsync();
            }
        }

        //        private async Task Listen(ServiceBusConnection connection, CancellationToken cancellationToken)
        private async Task Listen(CancellationToken cancellationToken)
        {
            var connection = new ServiceBusConnection(connectionString);
            var messageReceiver = new MessageReceiver(connection, EndpointName, ReceiveMode.PeekLock,
                RetryPolicy.Default, 0);
            var errorQueueSender = new MessageSender(connection, errorQueueName, RetryPolicy.Default);
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var pipeLineStopwatch = Stopwatch.StartNew();
                    var messages = new List<Message>();
                    for (var i = 0; i < 10 && messages.Count <= 200; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var receivedMessages = await messageReceiver.ReceiveAsync(200, TimeSpan.FromSeconds(2))
                            .ConfigureAwait(false);
                        if (receivedMessages == null || !receivedMessages.Any())
                            break;
                        messages.AddRange(receivedMessages);
                    }

                    if (!messages.Any())
                    {
                        await Task.Delay(2000, cancellationToken);
                        continue;
                    }

                    var groupedMessages = new Dictionary<Type, List<(string, object)>>();
                    foreach (var message in messages)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            var applicationMessage = DeserializeMessage(message);
                            var key = applicationMessage.GetType();
                            var applicationMessages = groupedMessages.ContainsKey(key)
                                ? groupedMessages[key]
                                : groupedMessages[key] = new List<(string, object)>();
                            applicationMessages.Add((message.SystemProperties.LockToken, applicationMessage));

                        }
                        catch (Exception e)
                        {
                            logger.LogError($"Error deserialising the message. Error: {e.Message}", e);
                            await errorQueueSender.SendAsync(message).ConfigureAwait(false);
                            await messageReceiver.CompleteAsync(message.SystemProperties.LockToken)
                                .ConfigureAwait(false);
                        }
                    }

                    var stopwatch = Stopwatch.StartNew();
                    await Task.WhenAll(groupedMessages.Select(group =>
                        ProcessMessages(group.Key, group.Value, messageReceiver, cancellationToken)));
                    stopwatch.Stop();
                    //RecordAllBatchProcessTelemetry(stopwatch.ElapsedMilliseconds, messages.Count);
                    pipeLineStopwatch.Stop();
                    //RecordPipelineTelemetry(pipeLineStopwatch.ElapsedMilliseconds, messages.Count);
                }
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning("Cancelling communication listener.");
                return;
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Cancelling communication listener.");
                return;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error listening for message.  Error: {ex.Message}", ex);
            }
            finally
            {
                if (!messageReceiver.IsClosedOrClosing)
                    await messageReceiver.CloseAsync();
                if (!connection.IsClosedOrClosing)
                    await connection.CloseAsync();
            }
        }

        private void RecordBatchProcessTelemetry(long elapsedMilliseconds, int count)
        {
            RecordMetric("BatchedServiceBusCommunicationListener.ProcessBatches", elapsedMilliseconds, count);
        }

        private void RecordAllBatchProcessTelemetry(long elapsedMilliseconds, int count)
        {
            RecordMetric("BatchedServiceBusCommunicationListener.ProcessAllBatches", elapsedMilliseconds, count);
        }

        private void RecordPipelineTelemetry(long elapsedMilliseconds, int count)
        {
            RecordMetric("BatchedServiceBusCommunicationListener.Pipeline", elapsedMilliseconds, count);
        }

        private void RecordMetric(string eventName, long elapsedMilliseconds, int count)
        {
            var metrics = new Dictionary<string, double>
            {
                {TelemetryKeys.Duration, elapsedMilliseconds},
                {TelemetryKeys.Count, count}
            };
            telemetry.TrackEvent(eventName, metrics);
        }

        private object DeserializeMessage(Message message)
        {
            if (!message.UserProperties.ContainsKey(NServiceBus.Headers.EnclosedMessageTypes))
                throw new InvalidOperationException($"Cannot deserialise the message, no 'enclosed message types' header was found. Message id: {message.MessageId}, label: {message.Label}");
            var enclosedTypes = (string)message.UserProperties[NServiceBus.Headers.EnclosedMessageTypes];
            var typeName = enclosedTypes.Split(';').FirstOrDefault();
            if (string.IsNullOrEmpty(typeName))
                throw new InvalidOperationException($"Message type not found when trying to deserialise the message.  Message id: {message.MessageId}, label: {message.Label}");
            var messageType = Type.GetType(typeName);
            var sanitisedMessageJson = GetMessagePayload(message);
            var deserialisedMessage = JsonConvert.DeserializeObject(sanitisedMessageJson, messageType);
            return deserialisedMessage;
        }

        protected async Task ProcessMessages(Type groupType, List<(string, object)> messages, MessageReceiver receiver,
            CancellationToken cancellationToken)
        {
            try
            {
                using (var containerScope = scopeFactory.CreateScope())
                {
                    var stopwatch = Stopwatch.StartNew();
                    var unitOfWork = containerScope.Resolve<IStateManagerUnitOfWork>();
                    try
                    {
                        await unitOfWork.Begin().ConfigureAwait(false);
                        var handler = containerScope.Resolve(typeof(IHandleMessageBatches<>).MakeGenericType(groupType));
                        var methodInfo = handler.GetType().GetMethod("Handle");
                        if (methodInfo == null)
                            throw new InvalidOperationException($"Handle method not found on handler: {handler.GetType().Name} for message type: {groupType.FullName}");
                        
                        var listType = typeof(List<>).MakeGenericType(groupType);
                        var list = (IList)Activator.CreateInstance(listType);
                        messages.Select(msg => msg.Item2).ToList().ForEach(message => list.Add(message));

                        await (Task)methodInfo.Invoke(handler, new object[] { list, cancellationToken });
                        await unitOfWork.End();
                        await receiver.CompleteAsync(messages.Select(message =>
                            message.Item1));
                    }
                    catch (Exception e)
                    {
                        await unitOfWork.End(e);
                        throw;
                    }
                    RecordBatchProcessTelemetry(stopwatch.ElapsedMilliseconds, messages.Count);
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error processing messages. Error: {e.Message}", e);
                await Task.WhenAll(messages.Select(message => receiver.AbandonAsync(message.Item1)));
            }
        }


        //private async Task ProcessMessage(Message receivedMessage, MessageReceiver messageReceiver,
        //    CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (!receivedMessage.UserProperties.ContainsKey(NServiceBus.Headers.EnclosedMessageTypes))
        //        {
        //            return;
        //        }

        //        var enclosedTypes = (string)receivedMessage.UserProperties[NServiceBus.Headers.EnclosedMessageTypes];
        //        var typeName = enclosedTypes.Split(';').FirstOrDefault();
        //        if (string.IsNullOrEmpty(typeName))
        //            return;
        //        var messageType = Type.GetType(typeName);
        //        var sanitisedMessageJson = GetMessagePayload(receivedMessage);
        //        var monitoringMessage = JsonConvert.DeserializeObject(sanitisedMessageJson, messageType);
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var handler = scope.Resolve(typeof(IHandleMessages<>).MakeGenericType(messageType));
        //            var methodInfo = handler.GetType().GetMethod("Handle");
        //            if (methodInfo == null)
        //                throw new InvalidOperationException($"Handle method not found on NSB handler: {handler.GetType().Name} for message type: {typeName}");
        //            await (Task)methodInfo.Invoke(handler, new object[] { monitoringMessage, null });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.LogError($"Error processing message. Error: {e.Message}", e);
        //        //await messageReceiver.AbandonAsync(receivedMessage.SystemProperties.LockToken);
        //        throw;
        //    }

        //}

        //private async Task ProcessMessages(List<Message> receivedMessages, MessageReceiver messageReceiver,
        //    CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var recordJobStatusMessages = receivedMessages
        //            .Select(msg =>
        //                JsonConvert.DeserializeObject<RecordJobMessageProcessingStatus>(GetMessagePayload(msg)))
        //            .ToList();

        //        using (var containerScope = scopeFactory.CreateScope())
        //        {
        //            var unitOfWorkScopeFactory = containerScope.Resolve<IUnitOfWorkScopeFactory>();
        //            using (var scope = unitOfWorkScopeFactory.Create($"JobService.RecordJobMessageProcessingStatus"))
        //            {
        //                var service = scope.Resolve<IJobMessageService>();
        //                await service.RecordCompletedJobMessageStatus(recordJobStatusMessages, cancellationToken);
        //                await scope.Commit();
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.LogError($"Error processing message. Error: {e.Message}", e);
        //        //await messageReceiver.AbandonAsync(receivedMessage.SystemProperties.LockToken);
        //        throw;
        //    }

        //}

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