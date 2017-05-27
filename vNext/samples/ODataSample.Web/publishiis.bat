@echo OFF
echo Stop the app pool so we can manipulate files and folders
call %windir%\system32\inetsrv\appcmd stop apppool "WebApiOData"
echo Clean up first
call rd "../Iis" /Q /S
echo Restore packages and build if necessary
call dotnet restore > restore.txt
echo Publish
call dotnet publish -c Release -o "../Iis"
echo Copy the main project's views and general www files
xcopy "wwwroot\*" "..\Iis\wwwroot\" /O /X /E /H /K /Y /C
echo Fire up the app pool again
call %windir%\system32\inetsrv\appcmd start apppool "WebApiOData"
echo Announce our success to the world!
powershell -c (New-Object Media.SoundPlayer "%windir%\media\notify.wav").PlaySync();
explorer http://localhost:5252/