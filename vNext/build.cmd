@echo off
pushd %~dp0
setlocal

echo.
echo dotnet version:
dotnet --version
if %ERRORLEVEL% neq 0 goto EnvFail

dotnet restore
if %ERRORLEVEL% neq 0 goto BuildFail

dotnet test test/Microsoft.AspNetCore.OData.Test/Microsoft.AspNetCore.OData.Test.csproj
if %ERRORLEVEL% neq 0 goto TestFail
goto BuildSuccess

:EnvFail
echo.
echo Please check dotnet command line tool is installed on your machine.
goto BuildFail

:TestFail
echo.
echo Please make all test cases passed.
goto BuildFail

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
