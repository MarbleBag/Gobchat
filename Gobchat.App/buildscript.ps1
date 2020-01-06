
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$7zipPath = "F:\Program Files\7-Zip\7z.exe"

$releaseFolder = Join-Path $scriptPath (Join-Path  "bin" "Release")
if(-Not (Test-Path $releaseFolder)){
	Write-Host "No release found"
	exit 1
}

#do a clean build

Write-Host "Deleting all the stuff no one cares about ..."
$allowedFolders = @("resources")
Get-ChildItem -Path $releaseFolder | 
	Where-Object {$_.PsIsContainer -eq $true} |
	ForEach-Object {
		if(-Not ($allowedFolders -match $_.Name)){
			Remove-Item -Recurse -Force  $_.FullName -ErrorAction SilentlyContinue
		}
	}

Get-ChildItem -Path $releaseFolder -Filter  *.log | 
	ForEach-Object {
		Remove-Item -Recurse -Force  $_.FullName -ErrorAction SilentlyContinue	
	}


Write-Host "Filling docs with important stuff ..."
$rld = "$releaseFolder\docs"
$ccc = @(
(New-Object PSObject -Property @{src="$PWD\..\docs\CHANGELOG.md";		dst="$rld\CHANGELOG.md"}),
(New-Object PSObject -Property @{src="$PWD\..\docs\LICENSE.md";			dst="$rld\LICENSE.md"}),
(New-Object PSObject -Property @{src="$PWD\..\docs\README.md";			dst="$rld\README.md"})
(New-Object PSObject -Property @{src="$PWD\..\docs\README_de.md";		dst="$rld\README_de.md"})
(New-Object PSObject -Property @{src="$PWD\..\Sharlayan\LICENSE.md";	dst="$rld\SHARLAYAN_LICENSE.md"})
)

New-Item -ItemType directory -Path $rld
$ccc |
	ForEach-Object {
		Copy-Item $_.src $_.dst
	}


$text = [System.IO.File]::ReadAllText("$PWD\Properties\AssemblyInfo.cs");
$hasMatch = $text -match '\[assembly: AssemblyVersion\("([0-9]+\.[0-9]+\.[0-9]+)\.[0-9]+"\)'
if(-Not $hasMatch){
	Write-Host "Unable to find app version"
	exit 1
}

$appVersion = $Matches[1]

Write-Host "Updating version number in about.html to $appVersion ..."
$aboutFile = "$releaseFolder\resources\ui\config\about.html"
(Get-Content $aboutFile) `
    -replace '\$\${appversion}', $appVersion |
  Out-File $aboutFile

Write-Host "Setting log level in nlog.config to info ..."
$nlogFile = "$releaseFolder\NLog.config"
(Get-Content $nlogFile) `
    -replace '<logger name="\*" minlevel=".+" writeTo="console" />', '<logger name="*" minlevel="off" writeTo="console" />'  `
	-replace '<logger name="\*" minlevel=".+" writeTo="file" />', '<logger name="*" minlevel="info" writeTo="file" />' |
	Out-File $nlogFile
  
Write-Host "Renaming release folder ..."
$gobFolder = Join-Path (Split-Path -parent $releaseFolder) "Gobchat"
if(Test-Path $gobFolder){
	Remove-Item -Recurse -Force  $gobFolder
}

Rename-Item -Path $releaseFolder -NewName "Gobchat"

if (-not (Test-Path -Path $7zipPath -PathType Leaf)) {
    throw "7zip not found at '$7zipPath' Unable to pack release"
}
#Set-Alias 7zip $7zipPath

$archiveName = "gobchat-$appVersion.zip"
$archive = Join-Path (Split-Path -parent $gobFolder) $archiveName
if (Test-Path $archive) {
	Write-Host "Deleting old archive"
	Remove-Item $archive
}

Write-Host "Packing release as $archiveName ..."
& $7zipPath a -mx=9 $archive $gobFolder

$outputLocation = (Split-Path -parent $scriptPath)
Write-Host "Moving package to $outputLocation ..."
Move-Item -Path $archive -Destination $outputLocation -Force

Write-Host "$archiveName ready to be released"