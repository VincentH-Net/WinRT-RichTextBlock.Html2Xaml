param($installPath, $toolsPath, $package, $project)

$projectFolder = Split-Path -Path $project.FullName -Parent
$csPath = Join-Path -Path $projectFolder -ChildPath "Common\RichTextBlockProperties.cs"
$csContent = [System.IO.File]::ReadAllText($csPath)
$defaultNamespace = $project.Properties.Item("DefaultNamespace").Value
$csContent = $csContent.Replace("RichTextBlockExtensions.Common", "$defaultNamespace.Common")
[System.IO.File]::WriteAllText($csPath, $csContent, [System.Text.Encoding]::UTF8)
