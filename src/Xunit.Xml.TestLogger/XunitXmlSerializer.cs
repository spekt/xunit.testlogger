// Copyright (c) Spekt Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.Extension.Xunit.Xml.TestLogger
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Spekt.TestLogger.Core;
    using Spekt.TestLogger.Utilities;

    public class XunitXmlSerializer : ITestResultSerializer
    {
        public const string EnvironmentKey = "Environment";
        public const string XUnitVersionKey = "XUnitVersion";

        private static List<string> errorTypes = new List<string>
        {
            "Test Assembly Cleanup Failure",
            "Test Collection Cleanup Failure",
            "Test Class Cleanup Failure",
            "Test Case Cleanup Failure",
            "Test Cleanup Failure",
            "Test Method Cleanup Failure"
        };

        private static Dictionary<string, string> errorTypeKeyValuePair = new Dictionary<string, string>
        {
            { "Test Assembly Cleanup Failure", "assembly-cleanup" },
            { "Test Collection Cleanup Failure", "test-collection-cleanup" },
            { "Test Class Cleanup Failure", "test-class-cleanup" },
            { "Test Case Cleanup Failure", "test-case-cleanup" },
            { "Test Cleanup Failure", "test-cleanup" },
            { "Test Method Cleanup Failure", "test-method-cleanup" }
        };

        public IInputSanitizer InputSanitizer { get; } = new InputSanitizerXml();

        public string Serialize(
            LoggerConfiguration loggerConfiguration,
            TestRunConfiguration runConfiguration,
            List<TestResultInfo> results,
            List<TestMessageInfo> messages)
        {
            var doc = new XDocument(CreateAssembliesElement(results, loggerConfiguration, runConfiguration));
            return doc.ToString();
        }

        private static XElement CreateAssembliesElement(
            List<TestResultInfo> results,
            LoggerConfiguration loggerConfiguration,
            TestRunConfiguration runConfiguration)
        {
            var assemblies = from result in results
                             group result by result.AssemblyPath into resultsByAssembly
                             orderby resultsByAssembly.Key
                             select CreateAssemblyElement(resultsByAssembly, loggerConfiguration, runConfiguration);
            var element = new XElement("assemblies", assemblies);

            element.SetAttributeValue("timestamp", runConfiguration.StartTime.ToString(CultureInfo.InvariantCulture));

            return element;
        }

        private static XElement CreateAssemblyElement(
            IGrouping<string, TestResultInfo> resultsByAssembly,
            LoggerConfiguration loggerConfiguration,
            TestRunConfiguration runConfiguration)
        {
            List<TestResultInfo> testResultAsError = new List<TestResultInfo>();
            var assemblyPath = resultsByAssembly.Key;

            var collections = from resultsInAssembly in resultsByAssembly
                              group resultsInAssembly by resultsInAssembly.FullTypeName into resultsByType
                              orderby resultsByType.Key
                              select CreateCollection(resultsByType, testResultAsError);

            int total = 0;
            int passed = 0;
            int failed = 0;
            int skipped = 0;
            int errors = 0;
            var time = runConfiguration.EndTime - runConfiguration.StartTime;

            var element = new XElement("assembly");
            XElement errorsElement = new XElement("errors");
            element.Add(errorsElement);

            foreach (var collection in collections)
            {
                total += collection.total;
                passed += collection.passed;
                failed += collection.failed;
                skipped += collection.skipped;
                errors += collection.error;

                element.Add(collection.element);
            }

            // Handle errors
            foreach (var error in testResultAsError)
            {
                errorsElement.Add(CreateErrorElement(error));
            }

            element.SetAttributeValue("name", assemblyPath);

            if (loggerConfiguration.Values.TryGetValue(EnvironmentKey, out var environment) &&
                !string.IsNullOrWhiteSpace(environment))
            {
                element.SetAttributeValue("environment", environment);
            }

            if (loggerConfiguration.Values.TryGetValue(XUnitVersionKey, out var version) &&
                !string.IsNullOrWhiteSpace(version))
            {
                element.SetAttributeValue("test-framework", "xUnit.net " + version);
            }

            element.SetAttributeValue("run-date", runConfiguration.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            element.SetAttributeValue("run-time", runConfiguration.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));

            var configFile = assemblyPath + ".config";
            if (File.Exists(configFile))
            {
                element.SetAttributeValue("config-file", configFile);
            }

            element.SetAttributeValue("total", total);
            element.SetAttributeValue("passed", passed);
            element.SetAttributeValue("failed", failed);
            element.SetAttributeValue("skipped", skipped);
            element.SetAttributeValue("time", time.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture));
            element.SetAttributeValue("errors", errors);

            return element;
        }

        private static XElement CreateErrorElement(TestResultInfo result)
        {
            string errorMessage = result.ErrorMessage;

            int indexOfErrorType = errorMessage.IndexOf('(');
            string errorType = string.Empty;
            errorTypeKeyValuePair.TryGetValue(errorMessage.Substring(1, indexOfErrorType - 2), out errorType);
            errorMessage = errorMessage.Substring(indexOfErrorType + 1);

            int indexOfName = errorMessage.IndexOf(')');
            string name = errorMessage.Substring(0, indexOfName);
            errorMessage = errorMessage.Substring(indexOfName + 4);

            int indexOfExceptionType = errorMessage.IndexOf(':');
            string exceptionType = errorMessage.Substring(0, indexOfExceptionType - 1);

            XElement errorElement = new XElement("error");
            errorElement.SetAttributeValue("type", errorType);
            errorElement.SetAttributeValue("name", name);

            errorElement.Add(CreateFailureElement(exceptionType, errorMessage, result.ErrorStackTrace));

            return errorElement;
        }

        private static XElement CreateFailureElement(string exceptionType, string message, string stackTrace)
        {
            XElement failureElement = new XElement("failure", new XAttribute("exception-type", exceptionType));
            failureElement.Add(new XElement("message", message));
            failureElement.Add(new XElement("stack-trace", stackTrace));

            return failureElement;
        }

        private static (XElement element, int total, int passed, int failed, int skipped, int error, TimeSpan time) CreateCollection(IGrouping<string, TestResultInfo> resultsByType, List<TestResultInfo> testResultAsError)
        {
            var element = new XElement("collection");

            int total = 0;
            int passed = 0;
            int failed = 0;
            int skipped = 0;
            int error = 0;
            var time = TimeSpan.Zero;

            foreach (var result in resultsByType)
            {
                switch (result.Outcome)
                {
                    case TestOutcome.Failed:
                        if (IsError(result))
                        {
                            if (!testResultAsError.Contains(result))
                            {
                                error++;
                                testResultAsError.Add(result);
                            }

                            continue;
                        }

                        failed++;
                        break;

                    case TestOutcome.Passed:
                        passed++;
                        break;

                    case TestOutcome.Skipped:
                        skipped++;
                        break;
                }

                total++;
                time += result.Duration;

                element.Add(CreateTestElement(result));
            }

            element.SetAttributeValue("total", total);
            element.SetAttributeValue("passed", passed);
            element.SetAttributeValue("failed", failed);
            element.SetAttributeValue("skipped", skipped);
            element.SetAttributeValue("name", $"Test collection for {resultsByType.Key}");
            element.SetAttributeValue("time", time.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture));

            return (element, total, passed, failed, skipped, error, time);
        }

        private static bool IsError(TestResultInfo result)
        {
            string errorMessage = result.ErrorMessage;
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                foreach (var m in errorTypes)
                {
                    if (errorMessage.IndexOf(m) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static XElement CreateTestElement(TestResultInfo result)
        {
            var element = new XElement(
                "test",
                new XAttribute("name", result.TestCaseDisplayName),
                new XAttribute("type", result.FullTypeName),
                new XAttribute("method", result.Method),
                new XAttribute("time", result.Duration.TotalSeconds.ToString("F7", CultureInfo.InvariantCulture)),
                new XAttribute("result", OutcomeToString(result.Outcome)));

            StringBuilder stdOut = new StringBuilder();
            foreach (var m in result.Messages)
            {
                if (TestResultMessage.StandardOutCategory.Equals(m.Category, StringComparison.OrdinalIgnoreCase))
                {
                    stdOut.AppendLine(m.Text);
                }
                else if (m.Category == "skipReason")
                {
                    // Using the self-defined category skipReason for now
                    element.Add(new XElement("reason", new XCData(m.Text)));
                }
            }

            if (!string.IsNullOrWhiteSpace(stdOut.ToString()))
            {
                element.Add(new XElement("output", stdOut.ToString()));
            }

            var fileName = result.TestCase.CodeFilePath;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                element.Add(new XElement("source-file", fileName));
                element.Add(new XElement("source-line", result.TestCase.LineNumber));
            }

            if (result.Outcome == TestOutcome.Failed)
            {
                element.Add(new XElement(
                    "failure",
                    new XElement("message", result.ErrorMessage),
                    new XElement("stack-trace", result.ErrorStackTrace)));
            }

            if (result.Traits != null)
            {
                var traits = from trait in result.Traits
                             select new XElement("trait", new XAttribute("name", trait.Name), new XAttribute("value", trait.Value));
                element.Add(new XElement("traits", traits));
            }

            return element;
        }

        private static string OutcomeToString(TestOutcome outcome)
        {
            switch (outcome)
            {
                case TestOutcome.Failed:
                    return "Fail";

                case TestOutcome.Passed:
                    return "Pass";

                case TestOutcome.Skipped:
                    return "Skip";

                default:
                    return "Unknown";
            }
        }
    }
}