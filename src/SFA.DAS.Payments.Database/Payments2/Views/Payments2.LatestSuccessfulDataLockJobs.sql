Create View [Payments2].[LatestSuccessfulDataLockJobs] 	As

With validJobs (DcJobId, Ukprn)
As
(
	Select
		Max(DcJobId),
		Ukprn
		From Payments2.Job
		Where status In (1,2,3)
		And DCJobSucceeded = 1
		Group By Ukprn
)
Select
	j.*
	From Payments2.Job j
	Join validJobs vj on j.DCJobId = vj.DcJobId

Go