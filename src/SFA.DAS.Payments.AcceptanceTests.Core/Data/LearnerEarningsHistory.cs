using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Tests.Core;

namespace SFA.DAS.Payments.AcceptanceTests.Core.Data
{
    public class LearnerEarningsHistory
    {
        private readonly AppEarnHistoryData appEarnHistoryData;
        private readonly List<Earning> previousEarnings;

        public LearnerEarningsHistory(AppEarnHistoryData appEarnHistoryData, IEnumerable<Earning> previousEarnings)
        {
            this.appEarnHistoryData = appEarnHistoryData ?? new AppEarnHistoryData();
            this.previousEarnings = previousEarnings?.ToList() ?? new List<Earning>();
        }

        public string CollectionYear => appEarnHistoryData.HistoryPeriod.ToAcademicYear();

        public int CollectionPeriod => appEarnHistoryData.HistoryPeriod.ToMonthPeriod();

        public int EmployerId => appEarnHistoryData.Employer.ToEmployerAccountId();

        public decimal? OnProgrammeEarningsToDate
        {
            get
            {
                return appEarnHistoryData.CapPreviousEarningsToHistoryPeriod ? CappedPreviousEarnings.Sum(x=>x.OnProgramme) : previousEarnings.Sum(x=>x.OnProgramme); 
            }
        }

        public decimal? TotalEarningsToDate => OnProgrammeEarningsToDate + BalancingEarningsToDate + CompletionEarningsToDate;

        public decimal? BalancingEarningsToDate
        {
            get { return appEarnHistoryData.CapPreviousEarningsToHistoryPeriod ? CappedPreviousEarnings.Sum(x => x.Balancing) : previousEarnings.Sum(x => x.Balancing); }
        }

        public decimal? CompletionEarningsToDate
        {
            get { return appEarnHistoryData.CapPreviousEarningsToHistoryPeriod ? CappedPreviousEarnings.Sum(x => x.Completion) : previousEarnings.Sum(x => x.Completion); }
        }

        public DateTime? UpToEndDate(string originalStartDate)
        {
            return originalStartDate.ToDate().Add(HistoryActualDuration(originalStartDate));
        }

        public int TrainingDaysCompleted(string originalStartDate)
        {
            return HistoryActualDuration(originalStartDate).Days + 1;
        }

        private TimeSpan HistoryActualDuration(string originalStartDate)
        {
            if (appEarnHistoryData.HistoryPeriod.Split('/').Length == 2)
            {
                return appEarnHistoryData.HistoryPeriod.ToDate().AddMonths(1) - originalStartDate.ToDate();
            } 
            
            if (string.IsNullOrWhiteSpace(originalStartDate) || string.IsNullOrWhiteSpace(appEarnHistoryData.ActualDuration))
            {
                return new TimeSpan();
            }

            return appEarnHistoryData.ActualDuration.ToTimeSpan(originalStartDate).HasValue
                       ? appEarnHistoryData.ActualDuration.ToTimeSpan(originalStartDate).Value
                       : new TimeSpan();
        }

        private List<Earning> CappedPreviousEarnings => previousEarnings?.Where(pe =>
                                                                             pe.DeliveryPeriod.ToAcademicYear() == CollectionYear &&
                                                                             pe.DeliveryPeriod.ToMonthPeriod() <= CollectionPeriod).ToList();
    }
}
