@ECHO OFF

%~dp0tools\GitHubFlowVersion\GitHubFlowVersion.exe /ProjectFile %~dp0GitReleaseNotes.proj /Targets Build;Package

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%

pause