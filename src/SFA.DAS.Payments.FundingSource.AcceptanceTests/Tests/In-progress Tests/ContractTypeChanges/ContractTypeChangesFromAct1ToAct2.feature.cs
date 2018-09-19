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
    [NUnit.Framework.DescriptionAttribute("Contract Type Changes From ACT1 To ACT2")]
    public partial class ContractTypeChangesFromACT1ToACT2Feature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ContractTypeChangesFromAct1ToAct2.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Contract Type Changes From ACT1 To ACT2", "\tDPP_965_02 - Non-Levy apprentice, provider edits contract type (ACT) in the ILR," +
                    " previous on-programme and English/math payments are refunded and repaid accordi" +
                    "ng to latest contract type", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 6
#line 7
 testRunner.Given("the current processing period is 3", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
 testRunner.And("a learner with LearnRefNumber learnref1 and Uln 10000 undertaking training with t" +
                    "raining provider 10000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table1.AddRow(new string[] {
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "-600"});
            table1.AddRow(new string[] {
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "-600"});
#line 11
 testRunner.And("the payments due component generates the following contract type 1 payable earnin" +
                    "gs:", ((string)(null)), table1, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "Amount"});
            table2.AddRow(new string[] {
                        "p2",
                        "1",
                        "10000",
                        "Learning_1",
                        "600"});
            table2.AddRow(new string[] {
                        "p2",
                        "2",
                        "10000",
                        "Learning_1",
                        "600"});
            table2.AddRow(new string[] {
                        "p2",
                        "3",
                        "10000",
                        "Learning_1",
                        "600"});
#line 16
 testRunner.And("the payments due component generates the following contract type 2 payable earnin" +
                    "gs:", ((string)(null)), table2, "And ");
#line hidden
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
#line 29
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 6
this.FeatureBackground();
#line 31
 testRunner.When("a payable earning event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "FundingSource",
                        "Amount"});
            table3.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "-540"});
            table3.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "-60"});
            table3.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "-540"});
            table3.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "-60"});
#line 33
 testRunner.Then("the payment source component will generate the following contract type 1 coinvest" +
                    "ed payments:", ((string)(null)), table3, "Then ");
#line hidden
            this.ScenarioCleanup();
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
#line 48
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 6
this.FeatureBackground();
#line 50
 testRunner.When("a payable earning event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "LearnRefNumber",
                        "Ukprn",
                        "PriceEpisodeIdentifier",
                        "Period",
                        "ULN",
                        "TransactionType",
                        "FundingSource",
                        "Amount"});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "540"});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "1",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "60"});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "540"});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "2",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "60"});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "3",
                        "10000",
                        "Learning_1",
                        "CoInvestedSfa_2",
                        "540"});
            table4.AddRow(new string[] {
                        "learnref1",
                        "10000",
                        "p1",
                        "3",
                        "10000",
                        "Learning_1",
                        "CoInvestedEmployer_3",
                        "60"});
#line 52
 testRunner.Then("the funding source component will generate the following contract type 2 coinvest" +
                    "ed payments:", ((string)(null)), table4, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
