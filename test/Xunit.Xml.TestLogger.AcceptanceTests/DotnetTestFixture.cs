namespace Xunit.Xml.TestLogger.AcceptanceTests
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class DotnetTestFixture : IDisposable
    {
        public DotnetTestFixture()
        {
            var testLogger = " --logger:'xunit;LogFilePath=test-results.xml'";
            var testProject = Path.Combine(Environment.CurrentDirectory, "../../../../Xunit.Xml.TestLogger.NetCore.Tests");
            using (var p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "dotnet";
                p.StartInfo.Arguments = "test --no-build " + testProject + testLogger;
                p.Start();

                // To avoid deadlocks, always read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                Console.WriteLine(testProject);
                Console.WriteLine(output);
            }
        }

        public void Dispose()
        {
        }
    }
}
