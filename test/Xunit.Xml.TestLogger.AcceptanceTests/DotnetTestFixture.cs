// Copyright (c) Spekt Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Xunit.Xml.TestLogger.AcceptanceTests
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class DotnetTestFixture : IDisposable
    {
        private const string DotnetVersion = "netcoreapp3.1";

        public DotnetTestFixture()
        {
            var testProject = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "assets", "Xunit.Xml.TestLogger.NetCore.Tests"));
            var testLogger = $"--logger:\"xunit;LogFilePath=test-results.xml\"";

            // Delete stale results file
            var testLogFile = Path.Combine(testProject, "test-results.xml");
            if (File.Exists(testLogFile))
            {
                File.Delete(testLogFile);
            }

            // Log the contents of test output directory. Useful to verify if the logger is copied
            Console.WriteLine("------------");
            Console.WriteLine("Contents of test output directory:");
            foreach (var f in Directory.GetFiles(Path.Combine(testProject, $"bin/Debug/{DotnetVersion}")))
            {
                Console.WriteLine("  " + f);
            }

            Console.WriteLine();

            // Run dotnet test with logger
            using (var p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "dotnet";
                p.StartInfo.Arguments = $"test --no-build {testLogger} {testProject}";

                // Required to run netcoreapp3.1 without icu support on devbox (linux)
                p.StartInfo.EnvironmentVariables.Add("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1");

                p.Start();

                Console.WriteLine("dotnet arguments: " + p.StartInfo.Arguments);

                // To avoid deadlocks, always read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                Console.WriteLine("dotnet output: " + output);
                Console.WriteLine("------------");
            }
        }

        public void Dispose()
        {
        }
    }
}
