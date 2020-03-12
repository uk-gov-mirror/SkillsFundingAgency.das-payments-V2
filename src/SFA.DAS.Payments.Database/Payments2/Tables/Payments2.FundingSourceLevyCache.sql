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
	Amount DECIMAL(15,5) NOT NULL,
	MessagePayload nvarchar(max) not null,
	MessageType nvarchar(max) not null
)
GO

CREATE NONCLUSTERED INDEX [IX_FundingSourceLevyCache__PeriodEnd] ON [Payments2].[FundingSourceLevyCache] 
([JobId], [AccountId], [TransferSenderAccountId]) include (Amount) 
WITH (ONLINE = ON)
GO