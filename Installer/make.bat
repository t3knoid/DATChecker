@echo off
REM This batch file checks if NSIS 2.51 has been installed. It is installed via Nuget in Visual Studio.
set makeDir=%~dp0
echo %makeDir%
cd "%makeDir%"
if not exist ..\packages\NSIS.2.51\tools\NSIS.exe @echo NSIS not found, exiting.&&exit /b 1
@echo NSIS exists. Making installer.
copy /y plugins\*.* ..\packages\NSIS.2.51\tools\plugins\.
..\packages\NSIS.2.51\tools\makensis.exe DatToXLS.nsi