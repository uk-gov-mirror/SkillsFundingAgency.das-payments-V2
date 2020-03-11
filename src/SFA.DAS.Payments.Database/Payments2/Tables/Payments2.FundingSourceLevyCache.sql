CREATE TABLE [Payments2].[FundingSourceLevyCache]
(
	Id BIGINT NOT NULL IDENTITY(1,1) CONSTRAINT PK_FundingSourceLevyCache PRIMARY KEY CLUSTERED,
	Ukprn BIGINT NOT NULL,
	CollectionPeriod TINYINT NOT NULL,
	AcademicYear SMALLINT NOT NULL,
	DeliveryPeriod TINYINT NOT NULL,
	JobId  BIGINT NOT NULL,
	[AccountId] BIGINT NULL, 
	TransferSenderAccountId BIGINT NULL, 
	RequiredPaymentEventId UNIQUEIDENTIFIER NOT NULL,
	EarningEventId UNIQUEIDENTIFIER NOT NULL,
	EventTime DATETIMEOFFSET NOT NULL,
	CreationDate DATETIMEOFFSET NOT NULL CONSTRAINT DF_FundingSourceLevyCache__CreationDate DEFAULT (SYSDATETIMEOFFSET()),
	MessagePayload nvarchar(max) not null
)
GO


CREATE NONCLUSTERED INDEX [IX_FundingSourceLevyCache__Submission] ON [Payments2].[RequiredPaymentEvent] 
([AcademicYear], [CollectionPeriod], [Ukprn], [IlrSubmissionDateTime]) 
WITH (ONLINE = ON)
GO

Create NONCLUSTERED INDEX [IX_RequiredPaymentEvent__Metrics] ON [Payments2].[RequiredPaymentEvent] 
(
	Ukprn,
	JobId,
	NonPaymentReason
) include (ContractType, TransactionType, Amount)
Go

CREATE INDEX IX_RequiredPaymentEvent__AcademicYear_CollectionPeriod_JobId
ON Payments2.RequiredPaymentEvent (AcademicYear, CollectionPeriod, JobId)
INCLUDE (EventId)
GO