dotnet pack
if ($?) {
    dotnet test test/Xunit.Xml.TestLogger.AcceptanceTests/Xunit.Xml.TestLogger.AcceptanceTests.csproj
}
