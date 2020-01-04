@echo off
powershell -ExecutionPolicy Unrestricted -File "%~dp0\generate-content-list.ps1"  -TargetFolder "%~dp0\bin\Release"
pause