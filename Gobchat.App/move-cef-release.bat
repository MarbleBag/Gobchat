@echo off
set Param1 = %~dp0\bin\Release\
set Param2 = %~dp0\bin\Release\libs\cef\
powershell -ExecutionPolicy Unrestricted -File "%~dp0\move-cef-release.ps1" -CefSource "%Param1%" -CefDestination "%Param2%"
pause