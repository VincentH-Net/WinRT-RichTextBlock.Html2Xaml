param($installPath, $toolsPath, $package, $project)

# Change the namespace in the cs file from WinRT_RichTextBlock.Html2Xaml to the Common sub-namespace within the project default namespace
$projectFolder = Split-Path -Path $project.FullName -Parent
$csPath = Join-Path -Path $projectFolder -ChildPath "Common\RichTextBlockProperties.cs"
$csContent = [System.IO.File]::ReadAllText($csPath)
$defaultNamespace = $project.Properties.Item("DefaultNamespace").Value
$csContent = $csContent.Replace("WinRT_RichTextBlock.Html2Xaml", "$defaultNamespace.Common")
[System.IO.File]::WriteAllText($csPath, $csContent, [System.Text.Encoding]::UTF8)

# Change the BuildAction of the xslt file to "Embedded Resource"
$item = $project.ProjectItems.Item("Common").ProjectItems.Item("RichTextBlockHtml2Xaml.xslt")
$item.Properties.Item("BuildAction").Value = 3;
