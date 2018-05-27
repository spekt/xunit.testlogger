namespace Xunit.Xml.TestLogger.AcceptanceTests
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Xunit;

    public class TestResultsXmlTests
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
            var timestamp = Convert.ToDateTime(assembliesNodes.Attributes["timestamp"].Value);
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

            Assert.Equal("4", assemblyNode.Attributes["total"].Value);
        }

        [Fact]
        public void AssemblyElementPassedAttributeShouldValueEqualToNumberOfPassedTests()
        {
            XmlNode assemblyNode = this.testResultsXmlDocument.SelectSingleNode(TestResultsXmlTests.AssemblyElement);

            Assert.Equal("2", assemblyNode.Attributes["passed"].Value);
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

            Assert.Equal("0", assemblyNode.Attributes["skipped"].Value);
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

            Assert.True(collectionElementNodeList.Count == 2);
        }

        [Fact]
        public void CollectionElementTotalAttributeShouldHaveValueEqualToTotalNumberOfTestsInAClass()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Assert.Equal("2", unitTest1Collection.Attributes["total"].Value);
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

            Assert.Equal("0", unitTest1Collection.Attributes["skipped"].Value);
        }

        [Fact]
        public void CollectionElementTimeAttributeShouldHaveValidFormatValue()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Regex regex = new Regex(@"^\d{1,}\.\d{3,3}$");
            Assert.Matches(regex, unitTest1Collection.Attributes["time"].Value);
        }

        [Fact]
        public void CollectionElementShouldContainsTwoTestsElemets()
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();

            Assert.True(unitTest1Collection.SelectNodes("test").Count == 2);
        }

        [Fact]
        public void TestElementTypeAttributeShouldHaveCorrectValue()
        {
            XmlNode failedTestXmlNode = GetFailedTestXmlNode();

            Assert.Equal("Xunit.Xml.TestLogger.NetCore.Tests.UnitTest1", failedTestXmlNode.Attributes["type"].Value);
        }

        [Fact]
        public void TestElementMethodAttributeShouldHaveCorrectValue()
        {
            XmlNode failedTestXmlNode = GetFailedTestXmlNode();

            Assert.Equal("FailTest11", failedTestXmlNode.Attributes["method"].Value);
        }

        [Fact]
        public void TestElementTimeAttributeShouldHaveValidFormatValue()
        {
            XmlNode failedTestXmlNode = GetFailedTestXmlNode();

            Regex regex = new Regex(@"^\d{1,}\.\d{7,7}$");

            Assert.Matches(regex, failedTestXmlNode.Attributes["time"].Value);
        }

        [Fact]
        public void TestElementShouldHaveTraits()
        {
            XmlNode failedTestXmlNode = GetFailedTestXmlNode();

            //TODO add traits to tests and update the assert.
            Assert.Equal(string.Empty, failedTestXmlNode.SelectSingleNode("traits").InnerText);
        }

        [Fact]
        public void FailedTestElementResultAttributeShouldHaveValueFail()
        {
            XmlNode failedTestXmlNode = GetFailedTestXmlNode();

            Assert.Equal("Fail", failedTestXmlNode.Attributes["result"].Value);
        }

        [Fact]
        public void PassedTestElementResultAttributeShouldHaveValuePass()
        {
            XmlNode passedTestXmlNode = GetFailedTestXmlNode("Xunit.Xml.TestLogger.NetCore.Tests.UnitTest1.PassTest11");

            Assert.Equal("Pass", passedTestXmlNode.Attributes["result"].Value);
        }

        [Fact]
        public void FailedTestElementShouldContainsFailureDetails()
        {
            XmlNode failedTestXmlNode = GetFailedTestXmlNode();

            var failureNodeList = failedTestXmlNode.SelectNodes("failure");

            Assert.True(failureNodeList.Count == 1);

            var failureXmlNode = failureNodeList[0];

            var expectedFailureMessage = "Assert.False() Failure" + Environment.NewLine + "Expected: False" +
                                         Environment.NewLine + "Actual:   True";
            Assert.Equal(expectedFailureMessage, failureXmlNode.SelectSingleNode("message").InnerText);

            // TODO why the stacktrace is empty? Bug in xunit, vstest or xunit logger?
            Assert.Equal(string.Empty, failureXmlNode.SelectSingleNode("stack-trace").InnerText);
        }

        private XmlNode GetFailedTestXmlNode(string queryTestName = "Xunit.Xml.TestLogger.NetCore.Tests.UnitTest1.FailTest11")
        {
            XmlNode unitTest1Collection = this.GetUnitTest1Collection();
            XmlNode failedTestXmlNode = null;

            var testNodes = unitTest1Collection.SelectNodes("test");
            Assert.True(testNodes.Count == 2);

            var testName = testNodes[0].Attributes["name"].Value;
            failedTestXmlNode = queryTestName.Equals(testName) ? testNodes[0] : testNodes[1];

            return failedTestXmlNode;
        }

        private XmlNode GetUnitTest1Collection()
        {
            XmlNodeList collectionElementNodeList =
                this.testResultsXmlDocument.SelectNodes(TestResultsXmlTests.CollectionElement);

            Assert.True(collectionElementNodeList.Count == 2);

            XmlNode unitTest1Collection = null;
            var firstCollectionName = collectionElementNodeList[0].Attributes["name"];
            unitTest1Collection = "Test collection for Xunit.Xml.TestLogger.NetCore.Tests.UnitTest1".Equals(firstCollectionName.Value)
                ? collectionElementNodeList[0] : collectionElementNodeList[1];

            return unitTest1Collection;
        }
    }
}
