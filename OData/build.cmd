@echo off
pushd %~dp0
setlocal

if exist bin goto build
mkdir bin

:Build

REM Find the most recent 32bit MSBuild.exe on the system. Require v12.0 (installed with VS2013) or later since .NET 4.0
REM is not supported. Also handle x86 operating systems, where %ProgramFiles(x86)% is not defined. Always quote the
REM %MSBuild% value when setting the variable and never quote %MSBuild% references.
set MSBuild="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBuild% @set MSBuild="%ProgramFiles%\MSBuild\12.0\Bin\MSBuild.exe"

if "%1" == "" goto BuildDefaults
if "%1" == "E2EV4" goto BuildE2EV4
if "%1" == "E2EV3" goto BuildE2EV3
if "%1" == "FULL" goto BuildDefaults

%MSBuild% WebApiOData.msbuild /m /nr:false /t:%* /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildDefaults
%MSBuild% WebApiOData.msbuild /m /nr:false /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail
if "%1" == "FULL" goto BuildE2EV4
goto BuildSuccess

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

if "%1" == "FULL" goto BuildE2EV3
goto BuildSuccess

:BuildE2EV3
echo *** E2EV3 Test ***
%MSBuild% WebApiOData.E2EV3.msbuild /m /nr:false /p:ResultFileName="test.result.xml" /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail
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
