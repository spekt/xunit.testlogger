# Xunit Test Logger
Xunit logger extensions for [Visual Studio Test Platform](https://gtihub.com/microsoft/vstest).

[![Build Status](https://travis-ci.com/Nipunam/xunit.testlogger.svg?branch=master)](https://travis-ci.com/Nipunam/xunit.testlogger)
[![Build status](https://ci.appveyor.com/api/projects/status/rac9mgpdslmffoqd?svg=true)](https://ci.appveyor.com/project/Nipunam/xunit-testlogger)

## Packages
| Logger | Nuget Package |
| ------ | ------------- |
| Xunit | [![NuGet](https://img.shields.io/nuget/v/XunitXml.TestLogger.svg)](https://www.nuget.org/packages/XunitXml.TestLogger/) |

## Usage
Xunit logger can generate xml reports in the xunit v2 format (https://xunit.github.io/docs/format-xml-v2.html).

1. Add a reference to the [Xunit Logger](https://www.nuget.org/packages/XunitXml.TestLogger) nuget package in test project
2. Use the following command line in tests
```
> dotnet test --test-adapter-path:. --logger:xunit
```
3. Test results are generated in the `TestResults` directory relative to the `test.csproj`

A path for the report file can be specified as follows:
```
> dotnet test --test-adapter-path:. --logger:xunit;LogFilePath=loggerFile.xml
```

`loggerFile.xml` will be generated in the same directory as `test.csproj`.

## LICENSE
MIT
