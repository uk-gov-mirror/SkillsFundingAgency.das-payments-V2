CREATE TABLE [Metrics].[Tolerance]
(
  Id int not null identity(1,1) constraint PK_Tollerance primary key clustered
  ,AcademicYear smallint not null
  ,CollectionPeriod tinyint not null
  ,SubmissionTolleranceMin decimal(15,5) not null
  ,SubmissionTolleranceMax decimal(15,5) not null
  ,PeriodEndToleranceMin decimal(15,5) not null
  ,PeriodEndToleranceMax decimal(15,5) not null
  ,Index IX_Tollerance__AcademicYear_CollectionPeriod (AcademicYear, CollectionPeriod)
  ,CONSTRAINT UQ_Tollerance Unique (AcademicYear, CollectionPeriod)
)
