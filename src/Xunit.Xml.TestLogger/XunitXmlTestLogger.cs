// Copyright (c) Spekt Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.Extension.Xunit.Xml.TestLogger
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Spekt.TestLogger;

    [FriendlyName(FriendlyName)]
    [ExtensionUri(ExtensionUri)]
    public class XunitXmlTestLogger : TestLogger
    {
        /// <summary>
        /// Uri used to uniquely identify the logger.
        /// </summary>
        public const string ExtensionUri = "logger://Microsoft/TestPlatform/XUnitXmlLogger/v1";

        /// <summary>
        /// Alternate user friendly string to uniquely identify the console logger.
        /// </summary>
        public const string FriendlyName = "xunit";

        public XunitXmlTestLogger()
            : base(new XunitXmlSerializer())
        {
        }

        protected override string DefaultTestResultFile => "TestResults.xml";
    }
}
