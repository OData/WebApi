@echo off
netsh http add urlacl http://+:50231/ user=%USERDOMAIN%\%USERNAME%
PAUSE