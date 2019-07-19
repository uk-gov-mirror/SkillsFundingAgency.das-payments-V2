﻿CREATE TABLE [Submissions].[SubmissionEvents]
(
	Id						bigint			PRIMARY KEY IDENTITY(1,1),
	IlrFileName				nvarchar(50)	NOT NULL,
	FileDateTime			datetime		NOT NULL,
	SubmittedDateTime		datetime		NOT NULL,
	ComponentVersionNumber	int				NOT NULL,
	UKPRN					bigint			NOT NULL,
	ULN						bigint			NOT NULL,
	LearnRefNumber			varchar(12)		NOT NULL,
    AimSeqNumber			INT				NOT NULL,
	PriceEpisodeIdentifier	varchar(25)		NOT NULL,
	StandardCode			bigint			NULL,
	ProgrammeType			int				NULL,
	FrameworkCode			int				NULL,
	PathwayCode				int				NULL,
	ActualStartDate			date			NULL,
	PlannedEndDate			date			NULL,
	ActualEndDate			date			NULL,
	OnProgrammeTotalPrice	decimal(15,5)	NULL,
	CompletionTotalPrice	decimal(15,5)	NULL,
	NINumber				varchar(9)		NULL,
	CommitmentId			bigint			NULL,
	AcademicYear			varchar(4)    	NOT NULL,
	EmployerReferenceNumber int             NULL, 
    [FundModel] SMALLINT NULL, 
    [AimType] TINYINT NULL, 
    [DelLocPostCode] VARCHAR(8) NULL, 
    [LearnActEndDate] DATE NULL, 
    [WithdrawReason] SMALLINT NULL, 
    [Outcome] TINYINT NULL
)
