namespace Xunit.Xml.TestLogger.AcceptanceTests
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class DotnetTestFixture : IDisposable
    {
        public DotnetTestFixture()
        {
            var testProject = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "Xunit.Xml.TestLogger.NetCore.Tests"));
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
            foreach (var f in Directory.GetFiles(Path.Combine(testProject, "bin/Debug/netcoreapp2.0")))
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
