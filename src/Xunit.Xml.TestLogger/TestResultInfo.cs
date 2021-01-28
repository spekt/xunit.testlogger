// Copyright (c) Spekt Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.Extension.Xunit.Xml.TestLogger
{
    using System;
    using System.Collections.ObjectModel;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    // Disabling warning CS0659: 'XunitXmlTestLogger.TestResultInfo' overrides Object.Equals(object o) but does not override Object.GetHashCode()
    // As this is a false alarm here.
#pragma warning disable 0659
#if NONE
    public class TestResultInfo
    {
        public TestResultInfo(
            TestCase testCase,
            TestOutcome outcome,
            string assemblyPath,
            string type,
            string method,
            string name,
            TimeSpan time,
            string errorMessage,
            string errorStackTrace,
            Collection<TestResultMessage> messages,
            TraitCollection traits)
        {
            this.TestCase = testCase;
            this.Outcome = outcome;
            this.AssemblyPath = assemblyPath;
            this.Type = type;
            this.Method = method;
            this.Name = name;
            this.Time = time;
            this.ErrorMessage = errorMessage;
            this.ErrorStackTrace = errorStackTrace;
            this.Messages = messages;
            this.Traits = traits;
        }

        public TestCase TestCase { get; private set; }

        public TestOutcome Outcome { get; private set; }

        public string AssemblyPath { get; private set; }

        public string Type { get; private set; }

        public string Method { get; private set; }

        public string Name { get; private set; }

        public TimeSpan Time { get; private set; }

        public string ErrorMessage { get; private set; }

        public string ErrorStackTrace { get; private set; }

        public Collection<TestResultMessage> Messages { get; private set; }

        public TraitCollection Traits { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is TestResultInfo)
            {
                TestResultInfo objectToCompare = (TestResultInfo)obj;
                if (string.Compare(this.ErrorMessage, objectToCompare.ErrorMessage) == 0 &&
                string.Compare(this.ErrorStackTrace, objectToCompare.ErrorStackTrace) == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
#endif
}