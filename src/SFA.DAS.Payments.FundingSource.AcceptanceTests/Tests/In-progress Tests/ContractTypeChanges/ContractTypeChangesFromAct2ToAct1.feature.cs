﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.4.0.0
//      SpecFlow Generator Version:2.4.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.Payments.FundingSource.AcceptanceTests.Tests.In_ProgressTests.ContractTypeChanges
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Contract Type Changes From ACT2 To ACT1")]
    public partial class ContractTypeChangesFromACT2ToACT1Feature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ContractTypeChangesFromAct2ToAct1.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Contract Type Changes From ACT2 To ACT1", "\tDPP_965_01 - Levy apprentice, provider edits contract type (ACT) in the ILR, pre" +
                    "vious on-programme and English/math payments are refunded and repaid according t" +
                    "o latest contract type", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 7
#line 8
 testRunner.Given("the current processing period is 3", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 10
 testRunner.And("a learner with LearnRefNumber learnref1 and Uln 10000 undertaking training with t" +
                    "raining provider 10000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table33 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table33.AddRow(new string[] {
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "-600"});
            table33.AddRow(new string[] {
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "-600"});
#line 12
 testRunner.And("the payments due component generates the following contract type 2 payable earnin" +
                    "gs:", ((string)(null)), table33, "And ");
#line hidden
            TechTalk.SpecFlow.Table table34 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table34.AddRow(new string[] {
                        "p2",
                        "1",
                        "10000",
                        "Learning_1",
                        "600"});
            table34.AddRow(new string[] {
                        "p2",
                        "2",
                        "10000",
                        "Learning_1",
                        "600"});
            table34.AddRow(new string[] {
                        "p2",
                        "3",
                        "10000",
                        "Learning_1",
                        "600"});
#line 17
 testRunner.And("the payments due component generates the following contract type 1 payable earnin" +
                    "gs:", ((string)(null)), table34, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 Learning payment")]
        [NUnit.Framework.CategoryAttribute("Non-DAS")]
        [NUnit.Framework.CategoryAttribute("minimum_tests")]
        [NUnit.Framework.CategoryAttribute("learner_changes_contract_type")]
        [NUnit.Framework.CategoryAttribute("apprenticeship_contract_type_changes")]
        [NUnit.Framework.CategoryAttribute("partial")]
        public virtual void ContractType2LearningPayment()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 Learning payment", null, new string[] {
                        "Non-DAS",
                        "minimum_tests",
                        "learner_changes_contract_type",
                        "apprenticeship_contract_type_changes",
                        "partial"});
#line 30
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 7
this.FeatureBackground();
#line 32
 testRunner.When("MASH is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table35 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "FundingSource",
                        "Amount"});
            table35.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "-540"});
            table35.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "-60"});
            table35.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "-540"});
            table35.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "-60"});
#line 34
 testRunner.Then("the payment source component will generate the following contract type 2 coinvest" +
                    "ed payments:", ((string)(null)), table35, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 1 Learning payment")]
        [NUnit.Framework.CategoryAttribute("DAS")]
        [NUnit.Framework.CategoryAttribute("minimum_tests")]
        [NUnit.Framework.CategoryAttribute("learner_changes_contract_type")]
        [NUnit.Framework.CategoryAttribute("apprenticeship_contract_type_changes")]
        [NUnit.Framework.CategoryAttribute("partial")]
        public virtual void ContractType1LearningPayment()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 1 Learning payment", null, new string[] {
                        "DAS",
                        "minimum_tests",
                        "learner_changes_contract_type",
                        "apprenticeship_contract_type_changes",
                        "partial"});
#line 49
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 7
this.FeatureBackground();
#line 51
 testRunner.When("MASH is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table36 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "FundingSource",
                        "Amount"});
            table36.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "540"});
            table36.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "60"});
            table36.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "540"});
            table36.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "60"});
            table36.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "3",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "540"});
            table36.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "3",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "60"});
#line 53
 testRunner.Then("the payment source component will generate the following contract type 1 coinvest" +
                    "ed payments:", ((string)(null)), table36, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
