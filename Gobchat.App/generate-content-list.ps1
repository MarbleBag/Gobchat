param(
    [string]$TargetFolder
    )
	
$TargetFolder = $TargetFolder.replace("\\", "\")
	
Write-Host ("Generating content list for updater of " + $TargetFolder)


if(![System.IO.Directory]::Exists($TargetFolder)){    
	Write-Host ("Folder not found!")
	return
}

$XmlOutput = [System.IO.Path]::Combine($TargetFolder, "GobFileContent.xml")
$XmlWriter = New-Object System.XML.XmlTextWriter($XmlOutput, $Null)
$XmlWriter.Formatting = [System.Xml.Formatting]::Indented
$XmlWriter.WriteStartDocument()


$XmlWriter.WriteStartElement("Content")
$XmlWriter.WriteStartElement("Files")

Get-ChildItem -Recurse -Path $TargetFolder   |
	ForEach-Object {
		$name = $_.FullName.replace($TargetFolder, "").trimStart('\').trimStart(' ')
		if($_ -is [System.IO.DirectoryInfo]){
			$xmlWriter.WriteElementString("Directory", $name)
		}elseif($_ -is [System.IO.FileInfo]){
			$xmlWriter.WriteElementString("File", $name)
		}
	}
	
$XmlWriter.WriteEndElement()
$XmlWriter.WriteEndElement()

$XmlWriter.Finalize
$XmlWriter.Flush()
$XmlWriter.Close()

Write-Host ("Done!")