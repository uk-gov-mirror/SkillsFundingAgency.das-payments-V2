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
namespace SFA.DAS.Payments.RequiredPayments.AcceptanceTests.Tests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Completion (TT2) only - On time completion")]
    public partial class CompletionTT2Only_OnTimeCompletionFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Completion (TT2) only - On time completion.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Completion (TT2) only - On time completion", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 3
#line 4
 testRunner.Given("the current processing period is 13", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 5
 testRunner.And("a learner is undertaking a training with a training provider", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 6
 testRunner.And("the payments are for the current collection year", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 7
 testRunner.And("the SFA contribution percentage is 90%", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Delivery Period",
                        "TransactionType",
                        "Amount"});
            table1.AddRow(new string[] {
                        "p1",
                        "1",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "2",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "3",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "4",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "5",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "6",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "7",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "8",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "9",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "10",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "11",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "12",
                        "Learning (TT1)",
                        "600"});
            table1.AddRow(new string[] {
                        "p1",
                        "13",
                        "Completion (TT2)",
                        "1800"});
#line 8
 testRunner.And("the earning events component generates the following contract type 2 earnings:", ((string)(null)), table1, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Delivery Period",
                        "TransactionType",
                        "Amount"});
            table2.AddRow(new string[] {
                        "p1",
                        "1",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "2",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "3",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "4",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "5",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "6",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "7",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "8",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "9",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "10",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "11",
                        "Learning (TT1)",
                        "600"});
            table2.AddRow(new string[] {
                        "p1",
                        "12",
                        "Learning (TT1)",
                        "600"});
#line 24
 testRunner.And("the following historical contract type 2 payments exist:", ((string)(null)), table2, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Contract Type 2 no On Programme Learning payments")]
        public virtual void ContractType2NoOnProgrammeLearningPayments()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Contract Type 2 no On Programme Learning payments", null, ((string[])(null)));
#line 39
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 3
this.FeatureBackground();
#line 40
 testRunner.When("an earning event is received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "PriceEpisodeIdentifier",
                        "Delivery Period",
                        "TransactionType",
                        "Amount"});
            table3.AddRow(new string[] {
                        "p1",
                        "13",
                        "Completion TT2",
                        "1800"});
#line 41
 testRunner.Then("the required payments component will only generate contract type 2 required payme" +
                    "nts", ((string)(null)), table3, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
