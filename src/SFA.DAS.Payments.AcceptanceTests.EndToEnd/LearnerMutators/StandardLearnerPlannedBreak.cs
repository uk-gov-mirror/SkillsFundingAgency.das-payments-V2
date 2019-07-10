using System;
using System.Collections.Generic;
using System.Linq;
using DCT.TestDataGenerator;
using TestGeneratorHelpers = DCT.TestDataGenerator.Helpers;
using ESFA.DC.ILR.Model.Loose;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;


namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.LearnerMutators
{
    public class StandardLearnerPlannedBreak : StandardLearner
    {
        public StandardLearnerPlannedBreak(IEnumerable<Learner> learners) : base(learners, "895")
        {
        }

        protected override void DoSpecificMutate(MessageLearner messageLearner, Learner learner)
        {
            base.DoSpecificMutate(messageLearner, learner);
            TestGeneratorHelpers.AddLearningDeliveryFAM(messageLearner, LearnDelFAMType.RES, LearnDelFAMCode.RES);

            var employmentStatus = messageLearner.LearnerEmploymentStatus[0];

            foreach (var statusMonitoring in employmentStatus.EmploymentStatusMonitoring)
            {
                if (statusMonitoring.ESMType == "EII")
                    statusMonitoring.ESMCode = 6;
            }
        }
    }
}