﻿using SFA.DAS.Payments.RequiredPayments.Domain.Entities;

namespace SFA.DAS.Payments.RequiredPayments.Domain
{
    public interface IPaymentDueProcessor
    {
        decimal CalculateRequiredPaymentAmount(decimal amountDue, Payment[] paymentHistory);
        decimal CalculateSfaContributionPercentage(decimal earningPercentage, decimal earningAmount, Payment[] paymentHistory);
    }
}