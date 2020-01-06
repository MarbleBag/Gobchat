
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

$releaseFolder = Join-Path $scriptPath (Join-Path  "bin" "Release")

#do a clean build

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

#add docs

$text = [System.IO.File]::ReadAllText("$PWD\Properties\AssemblyInfo.cs");
$hasMatch = $text -match '\[assembly: AssemblyVersion\("([0-9]+\.[0-9]+\.[0-9]+)\.[0-9]+"\)'
if(-Not $hasMatch){
	Write-Host "Unable to find app version"
	exit 1
}

$appVersion = $Matches[1]

Write-Host "Updating version number in about.html"
$aboutFile = "$releaseFolder\resources\ui\config\about.html"
(Get-Content $aboutFile) `
    -replace '\$\${appversion}', $appVersion |
  Out-File $aboutFile

Write-Host "Updating nlog.config"
$nlogFile = "$releaseFolder\NLog.config"
(Get-Content $nlogFile) `
    -replace '<logger name="\*" minlevel=".+" writeTo="console" />', '<logger name="*" minlevel="off" writeTo="console" />'  `
	-replace '<logger name="\*" minlevel=".+" writeTo="file" />', '<logger name="*" minlevel="info" writeTo="file" />' |
	Out-File $nlogFile
  

#rename folder

#zip It!

#move it to project root!