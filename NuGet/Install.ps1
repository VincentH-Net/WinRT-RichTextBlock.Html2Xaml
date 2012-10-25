param($installPath, $toolsPath, $package, $project)

$item = $project.ProjectItems.Item("DesignFactory.WebMatrix.Executer.dll")
# Set to "Copy Always"
$item.Properties.Item("CopyToOutputDirectory").Value = 1

# Change to "None"
$item.Properties.Item("BuildAction").Value = 0;
