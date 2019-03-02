// Copyright (c) Spekt Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.Extension.Xunit.Xml.TestLogger
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    [FriendlyName(FriendlyName)]
    [ExtensionUri(ExtensionUri)]
    public class XunitXmlTestLogger : ITestLoggerWithParameters
    {
        /// <summary>
        /// Uri used to uniquely identify the logger.
        /// </summary>
        public const string ExtensionUri = "logger://Microsoft/TestPlatform/XUnitXmlLogger/v1";

        /// <summary>
        /// Alternate user friendly string to uniquely identify the console logger.
        /// </summary>
        public const string FriendlyName = "xunit";

        public const string LogFilePathKey = "LogFilePath";
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

        private readonly object resultsGuard = new object();

        private string outputFilePath;

        private string environmentOpt;

        private string xunitVersionOpt;

        private List<TestResultInfo> results;

        private DateTime localStartTime;

#pragma warning restore 0659

        public void Initialize(TestLoggerEvents events, string testResultsDirPath)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            if (testResultsDirPath == null)
            {
                throw new ArgumentNullException(nameof(testResultsDirPath));
            }

            var outputPath = Path.Combine(testResultsDirPath, "TestResults.xml");
            this.InitializeImpl(events, outputPath);
        }

        public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.TryGetValue(LogFilePathKey, out string outputPath))
            {
                this.InitializeImpl(events, outputPath);
            }
            else if (parameters.TryGetValue(DefaultLoggerParameterNames.TestRunDirectory, out string outputDir))
            {
                this.Initialize(events, outputDir);
            }
            else
            {
                throw new ArgumentException($"Expected {LogFilePathKey} or {DefaultLoggerParameterNames.TestRunDirectory} parameter", nameof(parameters));
            }

            parameters.TryGetValue(EnvironmentKey, out this.environmentOpt);
            parameters.TryGetValue(XUnitVersionKey, out this.xunitVersionOpt);
        }

        /// <summary>
        /// Called when a test message is received.
        /// </summary>
        internal void TestMessageHandler(object sender, TestRunMessageEventArgs e)
        {
        }

        /// <summary>
        /// Called when a test result is received.
        /// </summary>
        internal void TestResultHandler(object sender, TestResultEventArgs e)
        {
            TestResult result = e.Result;

            if (TryParseName(result.TestCase.FullyQualifiedName, out var typeName, out var methodName, out _))
            {
                lock (this.resultsGuard)
                {
                    this.results.Add(new TestResultInfo(
                        result.TestCase,
                        result.Outcome,
                        result.TestCase.Source,
                        typeName,
                        methodName,
                        result.DisplayName,
                        result.Duration,
                        result.ErrorMessage,
                        result.ErrorStackTrace,
                        result.Messages,
                        result.TestCase.Traits));
                }
            }
        }

        /// <summary>
        /// Called when a test run is completed.
        /// </summary>
        internal void TestRunCompleteHandler(object sender, TestRunCompleteEventArgs e)
        {
            List<TestResultInfo> resultList;
            lock (this.resultsGuard)
            {
                resultList = this.results;
                this.results = new List<TestResultInfo>();
            }

            var doc = new XDocument(this.CreateAssembliesElement(resultList));

            // Create directory if not exist
            var loggerFileDirPath = Path.GetDirectoryName(this.outputFilePath);
            if (!Directory.Exists(loggerFileDirPath))
            {
                Directory.CreateDirectory(loggerFileDirPath);
            }

            using (var f = File.Create(this.outputFilePath))
            {
                doc.Save(f);
            }

            var resultsFileMessage = string.Format(CultureInfo.CurrentCulture, "Results File: {0}", this.outputFilePath);
            Console.WriteLine(resultsFileMessage);
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
            failureElement.Add(new XElement("message", RemoveInvalidXmlChar(message)));
            failureElement.Add(new XElement("stack-trace", RemoveInvalidXmlChar(stackTrace)));

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
                time += result.Time;

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
                foreach (var m in XunitXmlTestLogger.errorTypes)
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
                new XAttribute("name", result.Name),
                new XAttribute("type", result.Type),
                new XAttribute("method", result.Method),
                new XAttribute("time", result.Time.TotalSeconds.ToString("N7", CultureInfo.InvariantCulture)),
                new XAttribute("result", OutcomeToString(result.Outcome)));

            StringBuilder stdOut = new StringBuilder();
            foreach (var m in result.Messages)
            {
                if (TestResultMessage.StandardOutCategory.Equals(m.Category, StringComparison.OrdinalIgnoreCase))
                {
                    stdOut.AppendLine(m.Text);
                }
            }

            if (!string.IsNullOrWhiteSpace(stdOut.ToString()))
            {
                element.Add(new XElement("output", RemoveInvalidXmlChar(stdOut.ToString())));
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
                    new XElement("message", RemoveInvalidXmlChar(result.ErrorMessage)),
                    new XElement("stack-trace", RemoveInvalidXmlChar(result.ErrorStackTrace))));
            }

            if (result.Traits != null)
            {
                var traits = from trait in result.Traits
                             select new XElement("trait", new XAttribute("name", trait.Name), new XAttribute("value", trait.Value));
                element.Add(new XElement("traits", traits));
            }

            return element;
        }

        private static bool TryParseName(string testCaseName, out string metadataTypeName, out string metadataMethodName, out string metadataMethodArguments)
        {
            // This is fragile. The FQN is constructed by a test adapter.
            // There is no enforcement that the FQN starts with metadata type name.
            string typeAndMethodName;
            var methodArgumentsStart = testCaseName.IndexOf('(');

            if (methodArgumentsStart == -1)
            {
                typeAndMethodName = testCaseName.Trim();
                metadataMethodArguments = string.Empty;
            }
            else
            {
                typeAndMethodName = testCaseName.Substring(0, methodArgumentsStart).Trim();
                metadataMethodArguments = testCaseName.Substring(methodArgumentsStart).Trim();

                if (metadataMethodArguments[metadataMethodArguments.Length - 1] != ')')
                {
                    metadataTypeName = null;
                    metadataMethodName = null;
                    metadataMethodArguments = null;
                    return false;
                }
            }

            var typeNameLength = typeAndMethodName.LastIndexOf('.');
            var methodNameStart = typeNameLength + 1;

            if (typeNameLength <= 0 || methodNameStart == typeAndMethodName.Length)
            {
                // No typeName is available
                metadataTypeName = null;
                metadataMethodName = null;
                metadataMethodArguments = null;
                return false;
            }

            metadataTypeName = typeAndMethodName.Substring(0, typeNameLength).Trim();
            metadataMethodName = typeAndMethodName.Substring(methodNameStart).Trim();
            return true;
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

        private static string RemoveInvalidXmlChar(string str)
        {
            if (str != null)
            {
                // From xml spec (http://www.w3.org/TR/xml/#charsets) valid chars:
                // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]

                // we are handling only #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD]
                // because C# support unicode character in range \u0000 to \uFFFF
                MatchEvaluator evaluator = new MatchEvaluator(ReplaceInvalidCharacterWithUniCodeEscapeSequence);
                string invalidChar = @"[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD]";
                return Regex.Replace(str, invalidChar, evaluator);
            }

            return str;
        }

        private static string ReplaceInvalidCharacterWithUniCodeEscapeSequence(Match match)
        {
            return string.Format(@"\u{0:x4}", (ushort)match.Value[0]);
        }

        private void InitializeImpl(TestLoggerEvents events, string outputPath)
        {
            events.TestRunMessage += this.TestMessageHandler;
            events.TestResult += this.TestResultHandler;
            events.TestRunComplete += this.TestRunCompleteHandler;

            this.outputFilePath = Path.GetFullPath(outputPath);

            lock (this.resultsGuard)
            {
                this.results = new List<TestResultInfo>();
            }

            this.localStartTime = DateTime.Now;
        }

        private XElement CreateAssembliesElement(List<TestResultInfo> results)
        {
            var assemblies = from result in results
                             group result by result.AssemblyPath into resultsByAssembly
                             orderby resultsByAssembly.Key
                             select this.CreateAssemblyElement(resultsByAssembly);
            var element = new XElement("assemblies", assemblies);

            element.SetAttributeValue("timestamp", this.localStartTime.ToString(CultureInfo.InvariantCulture));

            return element;
        }

        private XElement CreateAssemblyElement(IGrouping<string, TestResultInfo> resultsByAssembly)
        {
            List<TestResultInfo> testResultAsError = new List<TestResultInfo>();
            var assemblyPath = resultsByAssembly.Key;

            var collections = from resultsInAssembly in resultsByAssembly
                              group resultsInAssembly by resultsInAssembly.Type into resultsByType
                              orderby resultsByType.Key
                              select CreateCollection(resultsByType, testResultAsError);

            int total = 0;
            int passed = 0;
            int failed = 0;
            int skipped = 0;
            int errors = 0;
            var time = TimeSpan.Zero;

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
                time += collection.time;

                element.Add(collection.element);
            }

            // Handle errors
            foreach (var error in testResultAsError)
            {
                errorsElement.Add(CreateErrorElement(error));
            }

            element.SetAttributeValue("name", assemblyPath);

            if (this.environmentOpt != null)
            {
                element.SetAttributeValue("environment", this.environmentOpt);
            }

            if (this.xunitVersionOpt != null)
            {
                element.SetAttributeValue("test-framework", "xUnit.net " + this.xunitVersionOpt);
            }

            element.SetAttributeValue("run-date", this.localStartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            element.SetAttributeValue("run-time", this.localStartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));

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
    }
}