GitHubReleaser
==============

Utility which makes it really easy to release your project on GitHub/NuGet

## The Idea
[GitHubFlowVersion](https://github.com/JakeGinnivan/GitHubFlowVersion) is the first step in releasing projects using SemVer really easily, by using it you can get proper SemVer compilant builds and also work in a continous delivery style if you want.

The next step is actually releasing, this project comes in after our CI builds and creates the NuGet package or other build artifacts.

This utility will ideally:

 - Tag your Git repository with the SemVer version number
 - Push that Tag to GitHub
 - Generate release notes (See issue #1 for this)
 - Create release on GitHub for the pushed Tag, putting the release notes in the release

Possible usage:

GitHubReleaser.exe /Repo JakeGinnivan/GitHubReleaser /Username JakeGinnivan /Password ... /Version 1.2.3

Jump into the discussions in the issues so we can make releasing from your build server a really simple automated process
