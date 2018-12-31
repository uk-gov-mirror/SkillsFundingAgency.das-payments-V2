﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Monitoring.Jobs.Data;
using SFA.DAS.Payments.Monitoring.Jobs.Data.Model;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Monitoring.Jobs.Application.UnitTests
{
    [TestFixture]
    public class ProviderEarningsJobServiceTests
    {
        private AutoMock mocker;
        private JobStepModel jobStep;
        private RecordJobMessageProcessingStatus jobMessageStatus;
        private List<JobStepModel> jobSteps;

        [SetUp]
        public void SetUp()
        {
            jobMessageStatus = new RecordJobMessageProcessingStatus
            {
                JobId = 1,
                MessageName = "Message1",
                Id = Guid.NewGuid(),
                Succeeded = true,
                EndTime = DateTimeOffset.UtcNow,
                GeneratedMessages = new List<GeneratedMessage>()
            };
            jobStep = new JobStepModel
            {
                JobId = jobMessageStatus.JobId,
                StartTime = DateTimeOffset.UtcNow.AddSeconds(-10),
                MessageName = "Message1",
                MessageId = jobMessageStatus.Id,
                Id = 2,
                Status = JobStepStatus.Queued,
            };
            mocker = AutoMock.GetLoose();
            var mockDataContext = mocker.Mock<IJobStatusDataContext>();
            mockDataContext.Setup(x => x.GetJobIdFromDcJobId(It.IsAny<long>()))
                .Returns(Task.FromResult<long>(99));

            jobSteps = new List<JobStepModel> { jobStep };
            mockDataContext
                .Setup(dc => dc.GetJobSteps(It.IsAny<List<Guid>>()))
                .Returns(Task.FromResult<List<JobStepModel>>(jobSteps));
            mockDataContext.Setup(dc => dc.GetJobIdFromDcJobId(It.IsAny<long>()))
                .Returns(Task.FromResult<long>(1));
        }

        [Test]
        public async Task Stores_New_Jobs()
        {
            var jobStarted = new RecordStartedProcessingProviderEarningsJob
            {
                CollectionPeriod = 1,
                CollectionYear = 1819,
                JobId = 1,
                Ukprn = 9999,
                IlrSubmissionTime = DateTime.UtcNow.AddMinutes(-20),
                StartTime = DateTimeOffset.UtcNow,
            };
            var service = mocker.Create<ProviderEarningsJobService>();
            await service.JobStarted(jobStarted);
            mocker.Mock<IJobStatusDataContext>()
                .Verify(dc => dc.SaveNewProviderEarningsJob(
                    It.Is<JobModel>(job =>
                        job.StartTime == jobStarted.StartTime && job.Status == Data.Model.JobStatus.InProgress),
                    It.Is<ProviderEarningsJobModel>(providerJob =>
                        providerJob.DcJobId == jobStarted.JobId && providerJob.CollectionPeriod == jobStarted.CollectionPeriod
                                                                && providerJob.CollectionYear == jobStarted.CollectionYear
                                                                && providerJob.IlrSubmissionTime == jobStarted.IlrSubmissionTime
                                                                && providerJob.Ukprn == jobStarted.Ukprn),
                    It.IsAny<List<JobStepModel>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private async Task JobStepCompleted()
        {
            var service = mocker.Create<ProviderEarningsJobService>();
            await service.JobStepCompleted(jobMessageStatus);
        }

        [Test]
        public async Task Records_Status_Of_Completed_JobStep()
        {
            await JobStepCompleted();
            mocker.Mock<IJobStatusDataContext>()
                .Verify(dc => dc.SaveJobSteps(It.Is<List<JobStepModel>>(list =>
                    list.Any(item =>
                        item.Id == jobStep.Id &&
                        item.MessageId == jobMessageStatus.Id &&
                        item.Status == JobStepStatus.Completed &&
                        item.EndTime == jobMessageStatus.EndTime))), Times.Once);
        }

        [Test]
        public async Task Creates_New_Completed_Step_Model_If_Not_Found()
        {
            jobSteps.Clear();
            await JobStepCompleted();
            mocker.Mock<IJobStatusDataContext>()
                .Verify(dc => dc.SaveJobSteps(It.Is<List<JobStepModel>>(list =>
                    list.Any(item =>
                        item.Id == 0 &&
                        item.MessageId == jobMessageStatus.Id &&
                        item.Status == JobStepStatus.Completed &&
                        item.EndTime == jobMessageStatus.EndTime))), Times.Once);
        }

        [Test]
        public async Task Records_Status_Of_Generated_Messages()
        {
            var generatedMessage = new GeneratedMessage
            {

                StartTime = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid(),
                MessageName = "MessageA",
            };
            jobMessageStatus.GeneratedMessages.Add(generatedMessage);
            await JobStepCompleted();
            mocker.Mock<IJobStatusDataContext>()
                .Verify(dc => dc.SaveJobSteps(It.Is<List<JobStepModel>>(list =>
                    list.Any(item =>
                        item.MessageId == generatedMessage.MessageId &&
                        item.Status == JobStepStatus.Queued &&
                        item.StartTime == generatedMessage.StartTime))), Times.Once);
        }

        [Test]
        public async Task Records_Start_Time_Of_Existing_Generated_Messages()
        {
            var generatedMessage = new GeneratedMessage
            {
                StartTime = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid(),
                MessageName = "MessageA",
            };
            jobMessageStatus.GeneratedMessages.Add(generatedMessage);
            jobSteps.Add(new JobStepModel
            {
                JobId = jobStep.JobId,
                Id = 1002,
                EndTime = DateTimeOffset.UtcNow,
                Status = JobStepStatus.Completed,
                MessageId = generatedMessage.MessageId,
                MessageName = generatedMessage.MessageName,

            });
            await JobStepCompleted();
            mocker.Mock<IJobStatusDataContext>()
                .Verify(dc => dc.SaveJobSteps(It.Is<List<JobStepModel>>(list =>
                    list.Any(item =>
                        item.ParentMessageId == jobMessageStatus.Id &&
                        item.MessageId == generatedMessage.MessageId &&
                        item.Status == JobStepStatus.Completed &&
                        item.StartTime == generatedMessage.StartTime))), Times.Once);

        }

        [Test]
        public async Task Uses_JobStatusStats_Service_Complete_Job_If_No_Generated_Messages()
        {
            jobMessageStatus.GeneratedMessages.Clear();
            await JobStepCompleted();
            mocker.Mock<IJobsStatusServiceFacade>()
                .Verify(facade => facade.JobStepsCompleted(It.Is<long>(id => id == jobStep.JobId)), Times.Once());
        }

    }
}