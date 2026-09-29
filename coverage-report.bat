@ECHO OFF

REM Install tools if not present
dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

REM Clean and build solution
dotnet restore SalesApi.sln
dotnet build SalesApi.sln --configuration Release --no-restore

REM Run tests with coverage. Uses the VSTest "XPlat Code Coverage" collector
REM (coverlet.collector, already referenced by all three test projects) instead
REM of the coverlet.msbuild /p:CollectCoverage properties, which only Unit's
REM csproj understands - that mismatch used to silently drop Integration and
REM Functional coverage entirely. Requires Docker running (Integration/
REM Functional use Testcontainers).
dotnet test SalesApi.sln --no-restore --verbosity normal ^
--collect:"XPlat Code Coverage" ^
--settings:coverlet.runsettings ^
--results-directory:./TestResults

REM Generate coverage report
reportgenerator ^
-reports:"./TestResults/**/coverage.cobertura.xml" ^
-targetdir:"./TestResults/CoverageReport" ^
-reporttypes:Html

REM Removing temporary files
rmdir /s /q bin 2>nul
rmdir /s /q obj 2>nul

echo.
echo Coverage report generated at TestResults/CoverageReport/index.html
pause
