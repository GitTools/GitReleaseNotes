@ECHO OFF

%~dp0src\.nuget\nuget.exe restore %~dp0src\GitReleaseNotes.sln


%~dp0tools\GitHubFlowVersion\GitHubFlowVersion.exe /ProjectFile %~dp0GitReleaseNotes.proj /Targets Build;Package

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%

pause