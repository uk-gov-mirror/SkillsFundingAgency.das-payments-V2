﻿using System;
using System.Collections.Generic;
using SFA.DAS.Payments.EarningEvents.Application.Interfaces;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.EarningEvents.Application.Services
{
    public class RedundancyEarningService : IRedundancyEarningService
    {
        private readonly IRedundancyEarningEventFactory redundancyEarningEventFactory;


        public RedundancyEarningService(IRedundancyEarningEventFactory redundancyEarningEventFactory)
        {
            this.redundancyEarningEventFactory = redundancyEarningEventFactory ?? throw new ArgumentNullException(nameof(redundancyEarningEventFactory));
        }

        public List<ApprenticeshipContractTypeEarningsEvent> SplitContractEarningByRedundancyDate(ApprenticeshipContractTypeEarningsEvent earningEvent, DateTime redundancyDate)
        {
            List<ApprenticeshipContractTypeEarningsEvent> splitResults = new List<ApprenticeshipContractTypeEarningsEvent>();


            var redundancyPeriod = GetPeriodFromDate(redundancyDate);  //get period from redundancy date

            //map earning event to correct redundancy type
            var redundancyEarningEvent = redundancyEarningEventFactory.CreateRedundancyContractType(earningEvent);

            //Clear correct periods from Earning event
            earningEvent.OnProgrammeEarnings.ForEach(ope =>
            {
                foreach (var onProgPeriod in ope.Periods)
                {
                    if (onProgPeriod.Period >= redundancyPeriod)
                        onProgPeriod.Amount = 0m;
                }
            });
            earningEvent.IncentiveEarnings.ForEach(ie =>
            {
                foreach (var incentivePeriod in ie.Periods)
                {
                    if (incentivePeriod.Period >= redundancyPeriod)
                        incentivePeriod.Amount = 0m;
                }
            });

            //set correct values for redundancy periods
            redundancyEarningEvent.OnProgrammeEarnings.ForEach(ope =>
            {
                foreach (var onProgPeriod in ope.Periods)
                {
                    if (onProgPeriod.Period >= redundancyPeriod)
                        onProgPeriod.SfaContributionPercentage = 1m;
                }
            });
            redundancyEarningEvent.IncentiveEarnings.ForEach(ie =>
            {
                foreach (var incentivePeriod in ie.Periods)
                {
                    if (incentivePeriod.Period >= redundancyPeriod)
                        incentivePeriod.SfaContributionPercentage = 1m;
                }
            });

            splitResults.Add(earningEvent);
            splitResults.Add(redundancyEarningEvent);
            
            return splitResults;
        }

        public List<ApprenticeshipContractTypeEarningsEvent> SplitContractEarningsByRedundancyDate(List<ApprenticeshipContractTypeEarningsEvent> earningEvents, DateTime redundancyDate)
        {
            List<ApprenticeshipContractTypeEarningsEvent> results = new List<ApprenticeshipContractTypeEarningsEvent>();
            foreach (var apprenticeshipContractTypeEarningsEvent in earningEvents)
            {
                results.AddRange(SplitContractEarningByRedundancyDate(apprenticeshipContractTypeEarningsEvent, redundancyDate));
            }
            return results;
        }

        public List<FunctionalSkillEarningsEvent> SplitFunctionSkillEarningByRedundancyDate(FunctionalSkillEarningsEvent functionalSkillEarning,
            DateTime priceEpisodeRedStartDate)
        {
            List<FunctionalSkillEarningsEvent> splitResults = new List<FunctionalSkillEarningsEvent>();
            var redundancyPeriod = GetPeriodFromDate(priceEpisodeRedStartDate);  //get period from redundancy date

            //map earning event to correct redundancy type
            var redundancyEarningEvent = redundancyEarningEventFactory.CreateRedundancyFunctionalSkillType(functionalSkillEarning);
            
            //clear functionalSkills period
            foreach (var earning in functionalSkillEarning.Earnings)
            {
                foreach (var period in earning.Periods)
                {
                    if (period.Period >= redundancyPeriod)
                    {
                        period.Amount = 0m;
                    }
                }
            } 
            
            //update redundancy earning period
            foreach (var earning in redundancyEarningEvent.Earnings)
            {
                foreach (var period in earning.Periods)
                {
                    if (period.Period >= redundancyPeriod)
                    {
                        period.SfaContributionPercentage = 1m;
                    }
                }
            }

            splitResults.Add(functionalSkillEarning);
            splitResults.Add(redundancyEarningEvent);

            return splitResults;
        }


        private byte GetPeriodFromDate(DateTime date)
        {
            byte period;
            var month = date.Month;

            if (month < 8)
            {
                period = (byte) (month + 5);
            }
            else
            {
                period = (byte) (month - 7);
            }
            return period;
        }
    }
}