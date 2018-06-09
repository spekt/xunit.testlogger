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
            var testLogger = $"--logger 'xunit;LogFilePath=test-results.xml'";
            using (var p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "dotnet";
                p.StartInfo.Arguments = $"test --no-build {testLogger} {testProject}";
                p.Start();

                // To avoid deadlocks, always read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                Console.WriteLine("------------");
                Console.WriteLine("dotnet arguments: " + p.StartInfo.Arguments);
                Console.WriteLine("dotnet output: " + output);
                Console.WriteLine("------------");
            }
        }

        public void Dispose()
        {
        }
    }
}
