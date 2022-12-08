@echo off
powershell -ExecutionPolicy Unrestricted -File "%~dp0\move-cef-release.ps1"  -CefSource "%~dp0\bin\Release\" -CefDestination "%~dp0\bin\Release\libs\cef\\"
pause