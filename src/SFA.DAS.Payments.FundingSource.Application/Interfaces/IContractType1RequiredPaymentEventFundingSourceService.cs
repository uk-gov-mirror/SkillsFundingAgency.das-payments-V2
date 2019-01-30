﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Payments.FundingSource.Messages.Events;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.FundingSource.Application.Interfaces
{
    public interface IContractType1RequiredPaymentEventFundingSourceService
    {
        Task RegisterRequiredPayment(ApprenticeshipContractType1RequiredPaymentEvent paymentEvent);
        Task<IReadOnlyCollection<FundingSourcePaymentEvent>> GetFundedPayments();
    }
}