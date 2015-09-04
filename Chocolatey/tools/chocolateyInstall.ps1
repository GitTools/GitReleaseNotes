$packageName = 'GitReleaseNotes'
$version = '__version__'
$url = 'https://github.com/GitTools/GitReleaseNotes/releases/download/__version__/GitReleaseNotes.__version__.zip'
$validExitCodes = @(0) 

Install-ChocolateyZipPackage "$packageName" "$url" "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"