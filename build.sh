#!/usr/bin/env sh
# vi: set tw=0

dotnet pack &&\
dotnet test test/Xunit.Xml.TestLogger.AcceptanceTests/Xunit.Xml.TestLogger.AcceptanceTests.csproj
