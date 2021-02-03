// Copyright (c) Spekt Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Xunit.Xml.TestLogger.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Xunit;

    public class TestResultsXmlTests : IClassFixture<DotnetTestFixture>
    {
        private const string AssembliesElement = @"/assemblies";
        private const string AssemblyElement = @"/assemblies/assembly";
        private const string CollectionElement = @"/assemblies/assembly/collection";

        private string testResultsFilePath;
        private XmlDocument testResultsXmlDocument;

        public TestResultsXmlTests()
        {
            // TODO Test should run for Desktop test results file(D:\g\public\xunit.testlogger\test\Xunit.Xml.TestLogger.NetFull.Tests\test-results.xml).
            var currentAssemblyLocation = typeof(TestResultsXmlTests).GetTypeInfo().Assembly.Location;
            this.testResultsFilePath = Path.Combine(
                currentAssemblyLocation,
                "..",
                "..",
                "..",
                "..",
                "..",
                "assets",
                "Xunit.Xml.TestLogger.NetCore.Tests",
                "test-results.xml");
            this.testResultsXmlDocument = new XmlDocument();
            this.testResultsXmlDocument.Load(this.testResultsFilePath);
        }

        [Fact]
        public void OnlyOneAssembliesElementShouldExists()
        {
            var assembliesNodes = this.testResultsXmlDocument.SelectNodes(TestResultsXmlTests.AssembliesElement);

            Assert.True(assembliesNodes.Count == 1);
        }

        [Fact]
        public void AssembliesElementShouldHaveTimestampAttribute()
        {
            var assembliesNodes = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssembliesElement);

            Assert.NotNull(assembliesNodes.Attributes["timestamp"]);
        }

        [Fact]
        public void AssembliesElementTimestampAttributeShouldHaveValidTimestamp()
        {
            var assembliesNodes = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssembliesElement);

            // Should not throw FormatException.
            var timestamp = assembliesNodes.Attributes["timestamp"].Value;
            Convert.ToDateTime(timestamp, CultureInfo.InvariantCulture);
        }

        [Fact]
        public void AssembliesElementTimestampAttributeValueShouldHaveCertainFormat()
        {
            XmlNode assembliesNodes = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssembliesElement);

            string timestampString = assembliesNodes.Attributes["timestamp"].Value;
            Regex regex = new Regex(@"^\d{2,2}/\d{2,2}/\d{4,4} \d{2,2}:\d{2,2}:\d{2,2}$");

            Assert.Matches(regex, timestampString);
        }

        [Fact]
        public void AssemblyElementShouldPresent()
        {
            var assemblyNodes = this.testResultsXmlDocument.SelectNodes(TestResultsXmlTests.AssemblyElement);

            Assert.True(assemblyNodes.Count == 1);
        }

        [Fact]
        public void AssemblyElementNameAttributeShouldHaveValueRootedPathToAssembly()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            XmlAttribute nameAttribute = assemblyNode.Attributes["name"];
            Assert.NotNull(nameAttribute);

            string nameValue = nameAttribute.Value;
            Assert.True(File.Exists(nameValue));
            Assert.True(Path.IsPathRooted(nameValue));

            Assert.Equal("Xunit.Xml.TestLogger.NetCore.Tests.dll", Path.GetFileName(nameValue));
        }

        [Fact]
        public void AssemblyElementRunDateAttributeShouldHaveValidFormatDate()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            XmlAttribute runDateAttribute = assemblyNode.Attributes["run-date"];
            Assert.NotNull(runDateAttribute);

            string runDateValue = runDateAttribute.Value;
            Regex regex = new Regex(@"^\d{4,4}-\d{2,2}-\d{2,2}$");

            Assert.Matches(regex, runDateValue);
        }

        [Fact]
        public void AssemblyElementRunDateAttributeShouldHaveValidDateValue()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            XmlAttribute runTimeAttribute = assemblyNode.Attributes["run-time"];
            Assert.NotNull(runTimeAttribute);

            string runTimeValue = runTimeAttribute.Value;
            Regex regex = new Regex(@"^\d{2,2}:\d{2,2}:\d{2,2}$");

            Assert.Matches(regex, runTimeValue);
        }

        [Fact]
        public void AssemblyElementTotalAttributeShouldValueEqualToNumberOfTotalTests()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            Assert.Equal("6", assemblyNode.Attributes["total"].Value);
        }

        [Fact]
        public void AssemblyElementPassedAttributeShouldValueEqualToNumberOfPassedTests()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            Assert.Equal("3", assemblyNode.Attributes["passed"].Value);
        }

        [Fact]
        public void AssemblyElementFailedAttributeShouldHaveValueEqualToNumberOfFailedTests()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            Assert.Equal("2", assemblyNode.Attributes["failed"].Value);
        }

        [Fact]
        public void AssemblyElementSkippedAttributeShouldHaveValueEqualToNumberOfSkippedTests()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            Assert.Equal("1", assemblyNode.Attributes["skipped"].Value);
        }

        [Fact]
        public void AssemblyElementErrorsAttributeShouldHaveValueEqualToNumberOfErrors()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            Assert.Equal("0", assemblyNode.Attributes["errors"].Value);
        }

        [Fact]
        public void AssemblyElementTimeAttributeShouldHaveValidFormatValue()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            Regex regex = new Regex(@"^\d{1,}\.\d{3,3}$");
            Assert.Matches(regex, assemblyNode.Attributes["time"].Value);
        }

        [Fact]
        public void ErrorsElementShouldHaveNoError()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            XmlNode errorsNode = assemblyNode.SelectSingleNode("errors");

            Assert.Equal(string.Empty, errorsNode.InnerText);
        }

        [Fact]
        public void CollectionElementsCountShouldBeTwo()
        {
            XmlNodeList collectionElementNodeList = this.testResultsXmlDocument.SelectNodes(TestResultsXmlTests.CollectionElement);

            Assert.Equal(3, collectionElementNodeList.Count);
        }

        [Fact]
        public void CollectionElementTotalAttributeShouldHaveValueEqualToTotalNumberOfTestsInAClass()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Assert.Equal("3", unitTest1Collection.Attributes["total"].Value);
        }

        [Fact]
        public void CollectionElementPassedAttributeShouldHaveValueEqualToPassedTestsInAClass()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Assert.Equal("1", unitTest1Collection.Attributes["passed"].Value);
        }

        [Fact]
        public void CollectionElementFailedAttributeShouldHaveValueEqualToFailedTestsInAClass()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Assert.Equal("1", unitTest1Collection.Attributes["failed"].Value);
        }

        [Fact]
        public void CollectionElementSkippedAttributeShouldHaveValueEqualToSkippedTestsInAClass()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Assert.Equal("1", unitTest1Collection.Attributes["skipped"].Value);
        }

        [Fact]
        public void CollectionElementTimeAttributeShouldHaveValidFormatValue()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Regex regex = new Regex(@"^\d{1,}\.\d{3,3}$");
            Assert.Matches(regex, unitTest1Collection.Attributes["time"].Value);
        }

        [Fact]
        public void CollectionElementShouldContainThreeTestsElements()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Assert.True(unitTest1Collection.SelectNodes("test").Count == 3);
        }

        [Fact]
        public void TestElementNameAttributeShouldBeEscaped()
        {
            var testNodes = this.GetTestXmlNodePartial(
                "UnitTest3",
                @"Xunit.Xml.TestLogger.NetCore.Tests.UnitTest3.TestInvalidName");

            Assert.Equal(
                "Xunit.Xml.TestLogger.NetCore.Tests.UnitTest3.TestInvalidName(input: \"Head\\u0080r\")",
                testNodes.Item(0).Attributes["name"].Value);
        }

        [Fact]
        public void TestElementTypeAttributeShouldHaveCorrectValue()
        {
            XmlNode failedTestXmlNode = this.GetATestXmlNode();

            Assert.Equal("Xunit.Xml.TestLogger.NetCore.Tests.UnitTest1", failedTestXmlNode.Attributes["type"].Value);
        }

        [Fact]
        public void TestElementMethodAttributeShouldHaveCorrectValue()
        {
            XmlNode failedTestXmlNode = this.GetATestXmlNode();

            Assert.Equal("FailTest11", failedTestXmlNode.Attributes["method"].Value);
        }

        [Fact]
        public void TestElementTimeAttributeShouldHaveValidFormatValue()
        {
            XmlNode failedTestXmlNode = this.GetATestXmlNode();

            Regex regex = new Regex(@"^\d{1,}\.\d{7,7}$");

            Assert.Matches(regex, failedTestXmlNode.Attributes["time"].Value);
        }

        [Fact]
        public void TestElementShouldHaveTraits()
        {
            XmlNode failedTestXmlNode = this.GetATestXmlNode();

            var traits = failedTestXmlNode.SelectSingleNode("traits")?.ChildNodes;
            Assert.NotNull(traits);
            Assert.Equal(1, traits.Count);
            Assert.Equal("Category", traits[0].Attributes["name"].Value);
            Assert.Equal("DummyCategory", traits[0].Attributes["value"].Value);
        }

        [Fact]
        public void FailedTestElementResultAttributeShouldHaveValueFail()
        {
            XmlNode failedTestXmlNode = this.GetATestXmlNode();

            Assert.Equal("Fail", failedTestXmlNode.Attributes["result"].Value);
        }

        [Fact]
        public void PassedTestElementResultAttributeShouldHaveValuePass()
        {
            XmlNode passedTestXmlNode = this.GetATestXmlNode(
                "UnitTest1",
                "Xunit.Xml.TestLogger.NetCore.Tests.UnitTest1.PassTest11");

            Assert.Equal("Pass", passedTestXmlNode.Attributes["result"].Value);
        }

        [Fact]
        public void FailedTestElementShouldContainsFailureDetails()
        {
            XmlNode failedTestXmlNode = this.GetATestXmlNode();

            var failureNodeList = failedTestXmlNode.SelectNodes("failure");

            Assert.True(failureNodeList.Count == 1);

            var failureXmlNode = failureNodeList[0];

            var expectedFailureMessage = "Assert.False() Failure" + Environment.NewLine + "Expected: False" +
                                         Environment.NewLine + "Actual:   True";
            Assert.Equal(expectedFailureMessage, failureXmlNode.SelectSingleNode("message").InnerText);

            // TODO why the stacktrace is empty? Bug in xunit, vstest or xunit logger?
            Assert.Equal(string.Empty, failureXmlNode.SelectSingleNode("stack-trace").InnerText);
        }

        [Fact]
        public void SkippedTestElementShouldContainSkippingReason()
        {
            XmlNode skippedTestNode = this.GetATestXmlNode(
                "UnitTest1",
                "Xunit.Xml.TestLogger.NetCore.Tests.UnitTest1.SkipTest11");
            var reasonNodes = skippedTestNode.SelectNodes("reason");

            Assert.Equal(1, reasonNodes.Count);

            var reasonNode = reasonNodes[0].FirstChild;
            Assert.IsType<XmlCDataSection>(reasonNode);

            XmlCDataSection reasonData = (XmlCDataSection)reasonNode;

            string expectedReason = "Skipped";
            Assert.Equal(expectedReason, reasonData.Value);
        }

        private XmlNode GetATestXmlNode(
            string collectionName = "UnitTest1",
            string queryTestName = "Xunit.Xml.TestLogger.NetCore.Tests.UnitTest1.FailTest11")
        {
            var unitTest1Collection = this.GetUnitTestCollection(collectionName);

            var testNodes = unitTest1Collection.SelectNodes($"test[@name=\"{queryTestName}\"]");
            return testNodes.Item(0);
        }

        private XmlNodeList GetTestXmlNodePartial(
            string collectionName,
            string testName)
        {
            var unitTest1Collection = this.GetUnitTestCollection(collectionName);

            var testNodes = unitTest1Collection.SelectNodes($"test[contains(@name, \"{testName}\")]");
            return testNodes;
        }

        private XmlNode GetUnitTestCollection(string name)
        {
            var testNodes = this.testResultsXmlDocument.SelectNodes(
                $"//assemblies/assembly/collection[contains(@name, \"{name}\")]");

            Assert.Equal(1, testNodes.Count);
            return testNodes.Item(0);
        }

        private XmlNode GetUnitTest1Collection()
        {
            return this.GetUnitTestCollection("UnitTest1");
        }
    }
}
