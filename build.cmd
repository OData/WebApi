@echo off
pushd %~dp0
setlocal

if exist bin goto build
mkdir bin

:Build

REM Find the most recent 32bit MSBuild.exe on the system. Require v12.0 (installed with VS2013) or later since .NET 4.0
REM is not supported. Also handle x86 operating systems, where %ProgramFiles(x86)% is not defined. Always quote the
REM %MSBuild% value when setting the variable and never quote %MSBuild% references.
set MSBuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBuild% @set MSBuild="%ProgramFiles%\MSBuild\14.0\Bin\MSBuild.exe"
set FullBuild=0

REM with no switches, run the full set of tests.
if /I "%1" == "" (
  set FullBuild=1
  goto BuildDefaults
)

REM support quick build, unit tests only.
if /I "%1" == "QUICK" (
  set FullBuild=0
  goto BuildDefaults
)

REM Continue to suport original switches for those
REM who might have scripts setup to use them.
if /I "%1" == "E2EV4" goto BuildE2EV4
if /I "%1" == "FULL" (
  set FullBuild=1
  goto BuildDefaults
)

REM Build a specified target
%MSBuild% WebApiOData.msbuild /m /nr:false /t:%* /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

REM Build product code and unit tests
:BuildDefaults
%MSBuild% WebApiOData.msbuild /m /nr:false /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail
if %FullBuild% neq 0 goto BuildE2EV4
goto BuildSuccess

REM Build product and V4 End to End tests
:BuildE2EV4
echo *** E2EV4 Test against KatanaSelf ***
PowerShell.exe -executionpolicy remotesigned -File tools\scripts\ReplaceAppConfigValue.ps1 test\E2ETest\WebStack.QA.Test.OData\App.config "Nuwa.DefaultHostTypes" "KatanaSelf"
PowerShell.exe -executionpolicy remotesigned -File tools\scripts\ReplaceAppConfigValue.ps1 test\E2ETest\WebStack.QA.Test.OData\App.config "Nuwa.KatanaSelfStartingPort" "9001"
%MSBuild% WebApiOData.E2E.msbuild /m /nr:false /p:ResultFileName="KatanaSelf.test.result.xml" /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail

REM echo *** E2EV4 Test against IIS ***
REM PowerShell.exe -executionpolicy remotesigned -File tools\scripts\ReplaceAppConfigValue.ps1 test\E2ETest\WebStack.QA.Test.OData\App.config "Nuwa.DefaultHostTypes" "IIS"
REM PowerShell.exe -executionpolicy remotesigned -File tools\scripts\ReplaceAppConfigValue.ps1 test\E2ETest\WebStack.QA.Test.OData\App.config "Nuwa.KatanaSelfStartingPort" "9023"
REM %MSBuild% WebApiOData.E2E.msbuild /m /nr:false /p:ResultFileName="IIS.test.result.xml" /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
REM if %ERRORLEVEL% neq 0 goto BuildFail

goto BuildSuccess

:BuildFail
echo.
echo *** BUILD FAILED ***
goto End

:BuildSuccess
echo.
echo **** BUILD SUCCESSFUL ***
goto end

:End
popd
endlocal
