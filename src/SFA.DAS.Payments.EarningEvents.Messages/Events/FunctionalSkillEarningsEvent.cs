﻿using System;
using System.Collections.ObjectModel;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    /// <summary>
    /// Earning events for sub aims such as Maths & English
    /// </summary>
    /// <seealso cref="EarningEvent" />
    public class FunctionalSkillEarningsEvent : EarningEvent
    {
        /// <summary>
        /// Gets or sets the earnings.
        /// </summary>
        /// <value>
        /// The earnings.
        /// </value>
        /// <seealso cref="SFA.DAS.Payments.Model.Core.Incentives.FunctionalSkillEarning" />
        /// <seealso cref="SFA.DAS.Payments.Model.Core.Incentives.FunctionalSkillType" />
        public ReadOnlyCollection<FunctionalSkillEarning> Earnings { get; set; }

        public DateTime StartDate { get; set; }
        
        public ContractType ContractType { get; set; }
    }
}