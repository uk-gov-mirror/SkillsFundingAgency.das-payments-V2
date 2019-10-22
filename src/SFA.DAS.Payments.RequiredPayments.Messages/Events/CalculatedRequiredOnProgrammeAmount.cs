﻿using SFA.DAS.Payments.Messages.Core;
using SFA.DAS.Payments.Model.Core.OnProgramme;

namespace SFA.DAS.Payments.RequiredPayments.Messages.Events
{
    public abstract class CalculatedRequiredOnProgrammeAmount : PeriodisedRequiredPaymentEvent, IMonitoredMessage
    {
        public decimal SfaContributionPercentage { get; set; }
        public OnProgrammeEarningType OnProgrammeEarningType { get; set; }
    }
}