CREATE Function [Metrics].[GetSubmissionsSummary] 
(
    @academicYear smallint, 
    @collectionPeriod tinyint
)
Returns Table
as
return 
(
Select	   
	  (Sum([ContractType1]+[ContractType2]) / Sum(EarningsDCContractType1+EarningsDCContractType2)) * 100 as Percentage
      ,Sum([ContractType1]) as ContractType1
      ,Sum([ContractType2]) as ContractType2
      ,Sum([DifferenceContractType1]) as DifferenceContractType1
      ,Sum([DifferenceContractType2]) as DifferenceContractType2
      ,Avg([PercentageContractType1]) as PercentageContractType1
      ,Avg([PercentageContractType2]) as PercentageContractType2
      ,Sum([EarningsDCContractType1])  as EarningsDCContractType1
      ,Sum([EarningsDCContractType2]) as EarningsDCContractType2
      ,Sum([EarningsDASContractType1]) as EarningsDASContractType1
      ,Sum([EarningsDASContractType2]) as EarningsDASContractType2
      ,Sum([EarningsDifferenceContractType1]) as EarningsDifferenceContractType1
      ,Sum([EarningsDifferenceContractType2]) as EarningsDifferenceContractType2
      ,Avg([EarningsPercentageContractType1]) as EarningsPercentageContractType1
      ,Avg([EarningsPercentageContractType2]) as EarningsPercentageContractType2
      ,Sum([RequiredPaymentsContractType1]) as RequiredPaymentsContractType1
      ,Sum([RequiredPaymentsContractType2]) as RequiredPaymentsContractType2
      ,Sum([AdjustedDataLockedEarnings]) as AdjustedDataLockedEarnings
      ,Sum([AlreadyPaidDataLockedEarnings]) as AlreadyPaidDataLockedEarnings
      ,Sum([TotalDataLockedEarnings]) as TotalDataLockedEarnings
      ,Sum([HeldBackCompletionPaymentsContractType1]) as HeldBackCompletionPaymentsContractType1
      ,Sum([HeldBackCompletionPaymentsContractType2]) as HeldBackCompletionPaymentsContractType2
      ,Sum([PaymentsYearToDateContractType1]) as PaymentsYearToDateContractType1
      ,Sum([PaymentsYearToDateContractType2]) as PaymentsYearToDateContractType2
  FROM [Metrics].[SubmissionSummary]
  where AcademicYear = @academicYear 
	and CollectionPeriod = @collectionPeriod
)
