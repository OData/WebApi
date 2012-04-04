@echo off
pushd %~dp0

if exist bin goto build
mkdir bin

:Build
if "%1" == "" goto BuildDefaults

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild Runtime.msbuild /m /nr:false /t:%* /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if errorlevel 1 goto BuildFail
goto BuildSuccess

:BuildDefaults
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild Runtime.msbuild /m /nr:false /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if errorlevel 1 goto BuildFail
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
