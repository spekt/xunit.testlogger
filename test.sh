echo "#############   Start pack ############"
# For local package source directory(./src/package/bin/Debug in Nuget.config) should present for restore successful.
mkdir -p ./src/package/bin/Debug
dotnet pack -p:"version=1.0.0-dev"

echo "############  Run E2E tests  ############"

dotnet restore -p:"LoggerVersion=1.0.0-dev" --configfile Nuget.config
dotnet build --no-restore
dotnet test --no-build ./test/Xunit.Xml.TestLogger.NetCore.Tests/ --logger:"xunit;LogFilePath=test-results.xml"
dotnet test --no-build ./test/Xunit.Xml.TestLogger.NetFull.Tests/ --logger:"xunit;LogFilePath=test-results.xml"

# Todo add validation 

