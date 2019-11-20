@echo off
powershell -ExecutionPolicy Unrestricted -File "%~dp0\move-cef-redists.ps1"  -CefSource "%~dp0\" -CefDestination "%~dp0\libs\cef\\"
pause