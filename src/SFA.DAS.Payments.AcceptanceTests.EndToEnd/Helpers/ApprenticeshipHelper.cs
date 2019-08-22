﻿using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Data;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Extensions;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Tests.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Helpers
{
    public static class ApprenticeshipHelper
    {
        public static async Task AddApprenticeship(ApprenticeshipModel apprenticeship, IPaymentsDataContext dataContext)
        {
            await dataContext.Apprenticeship.AddAsync(apprenticeship).ConfigureAwait(false);
            await dataContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public static async Task UpdateApprenticeship(long apprenticeshipId, ApprenticeshipStatus status, List<ApprenticeshipPriceEpisodeModel> priceEpisodes, IPaymentsDataContext dataContext)
        {
            var apprenticeship = await dataContext.Apprenticeship
                                     .Include(a => a.ApprenticeshipPriceEpisodes)
                                     .FirstOrDefaultAsync(a => a.Id == apprenticeshipId)
                                 ?? throw new InvalidOperationException($"Apprenticeship not found: {apprenticeshipId}");

            apprenticeship.Status = status;
            apprenticeship.ApprenticeshipPriceEpisodes.ForEach(priceEpisode => priceEpisode.Removed = true);

            apprenticeship.ApprenticeshipPriceEpisodes.AddRange(priceEpisodes.Where
                (pe=> !apprenticeship.ApprenticeshipPriceEpisodes.Any
                (ape=>ape.StartDate == pe.StartDate && ape.Cost == pe.Cost)));

            await dataContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public static ApprenticeshipModel CreateApprenticeshipModel(Apprenticeship apprenticeshipSpec, TestSession testSession)
        {
            if (apprenticeshipSpec.ApprenticeshipId == default(long)) apprenticeshipSpec.ApprenticeshipId = testSession.GenerateId();

            if (apprenticeshipSpec.Ukprn == default(long))
            {
                if (string.IsNullOrWhiteSpace(apprenticeshipSpec.Provider))
                {
                    apprenticeshipSpec.Ukprn = testSession.Ukprn;
                }
                else
                {
                    var suppliedProvider = testSession.GetProviderByIdentifier(apprenticeshipSpec.Provider);
                    apprenticeshipSpec.Ukprn = suppliedProvider.Ukprn;
                }
            }

            var employer = testSession.GetEmployer(apprenticeshipSpec.Employer);
            if (apprenticeshipSpec.AccountId == default(long))
                apprenticeshipSpec.AccountId = employer.AccountId;

            if (!string.IsNullOrEmpty(apprenticeshipSpec.SendingEmployer) && !apprenticeshipSpec.SenderAccountId.HasValue)
                apprenticeshipSpec.SenderAccountId = testSession.GetEmployer(apprenticeshipSpec.SendingEmployer).AccountId;

            if (apprenticeshipSpec.Uln == default(long))
            {
                var learnerId = !string.IsNullOrWhiteSpace(apprenticeshipSpec.LearnerId)
                    ? apprenticeshipSpec.LearnerId
                    : testSession.Learner.LearnerIdentifier;

                apprenticeshipSpec.Uln = testSession.GetLearner(apprenticeshipSpec.Ukprn, learnerId).Uln;
            }

            var apprenticeshipModel = new ApprenticeshipModel
            {
                Id = apprenticeshipSpec.ApprenticeshipId,
                Ukprn = apprenticeshipSpec.Ukprn,
                AccountId = apprenticeshipSpec.AccountId,
                TransferSendingEmployerAccountId = apprenticeshipSpec.SenderAccountId,
                Uln = apprenticeshipSpec.Uln,
                FrameworkCode = apprenticeshipSpec.FrameworkCode ?? 0, 
                ProgrammeType = apprenticeshipSpec.ProgrammeType ?? 0,
                PathwayCode = apprenticeshipSpec.PathwayCode ?? 0,
                StandardCode = apprenticeshipSpec.StandardCode ?? 0,
                Priority = apprenticeshipSpec.Priority,
                Status = apprenticeshipSpec.Status.ToApprenticeshipPaymentStatus(),
                LegalEntityName = "Test SFA",
                EstimatedStartDate = apprenticeshipSpec.StartDate.ToDate(),
                EstimatedEndDate = apprenticeshipSpec.EndDate.ToDate(),
                AgreedOnDate = string.IsNullOrWhiteSpace(apprenticeshipSpec.AgreedOnDate)?
                                DateTime.UtcNow :
                                apprenticeshipSpec.AgreedOnDate.ToDate(),
                StopDate = string.IsNullOrWhiteSpace(apprenticeshipSpec.StopEffectiveFrom) ?
                           default(DateTime?) :
                          apprenticeshipSpec.StopEffectiveFrom.ToDate(),
                IsLevyPayer = employer.IsLevyPayer,
            };

            return apprenticeshipModel;
        }

        public static ApprenticeshipPriceEpisodeModel CreateApprenticeshipPriceEpisode(Apprenticeship apprenticeshipSpec)
        {
            var startDate = string.IsNullOrWhiteSpace(apprenticeshipSpec.EffectiveFrom)
                ? apprenticeshipSpec.StartDate.ToDate()
                : apprenticeshipSpec.EffectiveFrom.ToDate();

            var endDate = string.IsNullOrWhiteSpace(apprenticeshipSpec.EffectiveTo)
                ? default(DateTime?)
                : apprenticeshipSpec.EffectiveTo.ToDate();

            return new ApprenticeshipPriceEpisodeModel
            {
                ApprenticeshipId = apprenticeshipSpec.ApprenticeshipId,
                Cost = apprenticeshipSpec.AgreedPrice,
                StartDate = startDate,
                EndDate = endDate,
                Removed = apprenticeshipSpec.Status.ToUpper() != "ACTIVE"
            };

        }

        public static ApprenticeshipDuplicateModel CreateApprenticeshipDuplicateModel(long ukprn, Apprenticeship apprenticeshipSpec)
        {
            var apprenticeshipModel = new ApprenticeshipDuplicateModel
            {
                Ukprn = ukprn,
                Uln = apprenticeshipSpec.Uln,
                ApprenticeshipId = apprenticeshipSpec.ApprenticeshipId
            };

            return apprenticeshipModel;
        }

        public static async Task AddApprenticeshipDuplicate(ApprenticeshipDuplicateModel apprenticeshipDuplicate, IPaymentsDataContext dataContext)
        {
            await dataContext.ApprenticeshipDuplicate.AddAsync(apprenticeshipDuplicate).ConfigureAwait(false);
            await dataContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
