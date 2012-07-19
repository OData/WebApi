@ECHO OFF
:CHECKFORSWITCHES
IF '%1'=='/h' GOTO DISPINFO
IF '%1'=='/H' GOTO DISPINFO
IF '%1'=='/?' GOTO DISPINFO
IF '%1'=='/register' GOTO REGISTER
IF '%1'=='/unregister' GOTO UNREGISTER
IF '%1'=='/REGISTER' GOTO REGISTER
IF '%1'=='/UNREGISTER' GOTO UNREGISTER

GOTO DISPINFO

:REGISTER
netsh http add urlacl http://+:50231/ user=%USERDOMAIN%\%USERNAME%
GOTO END

:UNREGISTER
netsh http delete urlacl http://+:50231/
GOTO END

:DISPINFO
ECHO Reserve/Unreserve http.sys url's used by aspnetwebstack unit tests
ECHO.
ECHO Syntax: TestHttpNamespace [/register] [/unregister] [/h]
GOTO END

:END
