function CreatePathSibling([string] $Path, [string] $Sibling){
	return Join-Path (Split-Path -parent $Path) $Sibling
}

function DeleteIfExists([string] $Path){
	if(Test-Path $Path){
		Write-Host "Deleting: $Path"
		Remove-Item -Recurse   $Path
	}
}

function MakeAndDeleteDirectory([string] $Path){
	DeleteIfExists $Path
	New-Item -Path $Path -ItemType directory
}

function RenameAndDeleteDirectory([string] $Path, [string] $NewName){
	$NewPath = CreatePathSibling $Path $NewName
	DeleteIfExists $NewPath
	Rename-Item -Path $Path -NewName $NewName
	return $NewPath
}


$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$7zipPath = "F:\Program Files\7-Zip\7z.exe"

$releaseFolder = Join-Path $scriptPath (Join-Path  "bin" "Release")
if(-Not (Test-Path $releaseFolder)){
	Write-Error "No release found"
	exit 1
}

#Remove any old build
Write-Host "Renaming release folder ..."
$releaseFolder = RenameAndDeleteDirectory $releaseFolder "Gobchat"
$debugFolder = CreatePathSibling $releaseFolder "GobchatDebug"
MakeAndDeleteDirectory $debugFolder

if(-Not (Test-Path $releaseFolder)){
	Write-Error "No gobchat folder"
	exit 1
}

#Deletes all folders except for the ones named in #allowedFolders
Write-Host "Deleting all the stuff no one cares about ..."
$allowedFolders = @("resources", "sharlayan", "de", "en")
Get-ChildItem -Path $releaseFolder | 
	Where-Object {$_.PsIsContainer -eq $true} |
	ForEach-Object {
		if(-Not ($allowedFolders -match $_.Name)){
			Remove-Item -Recurse -Force  $_.FullName -ErrorAction SilentlyContinue
		}
	}

#Delete downloadable content	
Remove-Item -Recurse -Force "$releaseFolder\resources\sharlayan" -ErrorAction SilentlyContinue

#Remove all .log files
Get-ChildItem -Path $releaseFolder -Filter  *.log | 
	ForEach-Object {
		Remove-Item -Recurse -Force  $_.FullName -ErrorAction SilentlyContinue	
	}
	
#Move any .pdb files
Get-ChildItem -Path $releaseFolder -Filter  *.pdb | 
	ForEach-Object {
		Move-Item -Path $_.FullName -Destination $debugFolder -Force -ErrorAction SilentlyContinue
	}

try{
	#copy documents to release
	Write-Host "Filling docs with important stuff ..."
	$rld = "$releaseFolder\docs"
	$ccc = @(
	(New-Object PSObject -Property @{src="$PWD\..\docs\CHANGELOG.pdf";		dst="$rld\CHANGELOG.pdf"}),
	(New-Object PSObject -Property @{src="$PWD\..\docs\LICENSE.md";			dst="$rld\LICENSE.md"}),
	(New-Object PSObject -Property @{src="$PWD\..\docs\README.pdf";			dst="$rld\README.pdf"})
	(New-Object PSObject -Property @{src="$PWD\..\docs\README_de.pdf";		dst="$rld\README_de.pdf"})
	(New-Object PSObject -Property @{src="$PWD\..\Sharlayan\LICENSE.md";	dst="$rld\SHARLAYAN_LICENSE.md"})
	)

	New-Item -ItemType directory -Path $rld
	$ccc |
		ForEach-Object {
			Copy-Item $_.src $_.dst -errorAction stop
		}
		
}catch{
	Write-Error $_
	exit 1
}

#generate content list
& "$scriptPath\generate-content-list.ps1" $releaseFolder

function GetApplicationVersion(){
	$text = [System.IO.File]::ReadAllText("$PWD\Properties\AssemblyInfo.cs");
	$hasMatch = $text -match '\[assembly: AssemblyVersion\("([0-9]+\.[0-9]+\.[0-9]+)\.([0-9]+)"\)'
	if(-Not $hasMatch){
		Write-Error "Unable to find app version"
		exit 1
		return $null
	}
	
	$appVersion = $Matches[1]
	$appPrerelease = $Matches[2]
	if( [int]::Parse($appPrerelease) -gt 0 ){
		$appVersion = "$appVersion-$appPrerelease"
	}
	
	return $appVersion
}

$appVersion = GetApplicationVersion
if (!$appVersion) { 
	Write-Error "Unable to find app version"
	exit 1
}

#Write-Host "Updating version number in about.html to $appVersion ..."
#$aboutFile = "$releaseFolder\resources\ui\config\about.html"
#(Get-Content $aboutFile) `
#    -replace '\$\${appversion}', $appVersion |
#  Out-File $aboutFile -encoding utf8

Write-Host "Setting log level in nlog.config to info ..."
Remove-Item -Force "$releaseFolder\NLog.config" -ErrorAction SilentlyContinue
Rename-Item -Path "$releaseFolder\NLog-Release.config" -NewName "NLog.config"

#(Get-Content $nlogFile) `
#    -replace '<logger name="\*" minlevel=".+" writeTo="console" />', '<logger name="*" minlevel="off" writeTo="console" />'  `
#	-replace '<logger name="\*" minlevel=".+" writeTo="file" />', '<logger name="*" minlevel="info" writeTo="file" />' |
#	Out-File $nlogFile -encoding utf8
  
if (-not (Test-Path -Path $7zipPath -PathType Leaf)) {
    throw "7zip not found at '$7zipPath' Unable to pack release"
}
#Set-Alias 7zip $7zipPath

$archiveRelease = CreatePathSibling $releaseFolder "gobchat-$appVersion.zip"
$archiveDebug = CreatePathSibling $debugFolder "gobchat-debug-$appVersion.zip"

DeleteIfExists $archiveRelease
DeleteIfExists $archiveDebug

Write-Host "Packing release as $archiveName ..."
& $7zipPath a -mx=9 $archiveRelease $releaseFolder
& $7zipPath a -mx=9 $archiveDebug $debugFolder

$outputLocation = (Split-Path -parent $scriptPath)
Write-Host "Moving package to $outputLocation ..."
Move-Item -Path $archiveRelease -Destination $outputLocation -Force
Move-Item -Path $archiveDebug -Destination $outputLocation -Force

Write-Host "$archiveName ready for release"