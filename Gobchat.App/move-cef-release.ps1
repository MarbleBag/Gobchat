param(
    [string]$CefSource, 
    [string]$CefDestination
    )
    
Write-Host ("Moving CEF folders from '" + $CefSource +"' to '" + $CefDestination +"'")

function MoveFileIfExists([string]$source, [string]$target){
    Write-Host ("Checking for folder: " + $source)
    if([System.IO.Directory]::Exists($source)){    
        Write-Host ("Move '" + $source + "' to '" + $target + "'")   
        [System.IO.Directory]::CreateDirectory($target) 
        Move-Item -Path $source -Destination $target -force
    }
}

try{
    $Source = $CefSource + "x86"
    MoveFileIfExists $Source $CefDestination
}catch {
    Write-Error $Error[0]
}

try{
    $Source = $CefSource + "x64"
    MoveFileIfExists $Source $CefDestination
}catch {
    Write-Error $Error[0]
}