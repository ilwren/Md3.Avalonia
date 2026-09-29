@echo off
setlocal
where powershell.exe >nul 2>nul
if errorlevel 1 (
  echo error: powershell.exe was not found. 1>&2
  exit /b 1
)

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-nuget.ps1" %*
exit /b %errorlevel%
