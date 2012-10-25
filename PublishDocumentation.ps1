# PublishDocumentation.ps1 - Create documentation page for a GitHub project from the current branch
# Author: Serge van den Oever [Macaw]
# Given a README.md in the current branch, publish a documentation page index.html on a gh-pages branch
# using http://www.DocumentUp.com where the content of the page is completely included in index.html
# to optimize SEO.

# Create a temporary directory (http://joelangley.blogspot.co.uk/2009/06/temp-directory-in-powershell.html)
function CreateTempDir
{
   $tmpDir = [System.IO.Path]::GetTempPath()
   $tmpDir = [System.IO.Path]::Combine($tmpDir, [System.IO.Path]::GetRandomFileName())
  
   [System.IO.Directory]::CreateDirectory($tmpDir) | Out-Null

   $tmpDir
}

# Checkout gh-pages branch to temp folder
$ghpagesRepoFolder = CreateTempDir
if (-not (Test-Path -Path $ghpagesRepoFolder))
{
    Throw "Failed to create temporary folder: $ghpagesRepoFolder"
}

Write-Host "Temporary folder for gh-pages repository: $ghpagesRepoFolder"


# Publish the documentation to the repository
git add README.md
git commit -m "Updated the documentation"
git push

# Get the url of the repository
$remoteGitRepositoryUrl = git config --get remote.origin.url
if ($remoteGitRepositoryUrl -eq $null)
{
    Throw "Execute this script within the folder of a GIT repository."
}

$currentGitBranch = (git symbolic-ref HEAD).split('/')[-1]

# Ensure the creation of a gh-page branch for the documentation pages
git ls-remote --exit-code . origin/gh-pages
if (-not $?)
{
    # Repository does not exist yet, create one from the current one
    git push origin HEAD:refs/heads/gh-pages
}

Push-Location $ghpagesRepoFolder
Write-Host "Changed location to temporary folder: $ghpagesRepoFolder"

Write-Host "Cloning gh-pages branch from $remoteGitRepositoryUrl to temporary folder"
git clone $remoteGitRepositoryUrl --branch gh-pages --single-branch $ghpagesRepoFolder
git checkout --orphan gh-pages

Write-Host "Cleanup all files except the index.html file"

$restoreIndexHtml = -not (Test-Path -Path "index.html")
git rm -rf *
if ($restoreIndexHtml)
{
    git reset index.html
}

# $remoteGitRepositoryUrl is in format https://github.com/MacawNL/WebMatrix.Executer.git
$githubBaseUrl = $remoteGitRepositoryUrl.Replace(".git", "")
$documentupBaseUrl = $githubBaseUrl.Replace("https://github.com", "http://documentup.com")

# Open the current documentation in a browser window
[System.Diagnostics.Process]::Start($githubBaseUrl + "/blob/$currentGitBranch/README.md")

# Force recompile of documentation using http://documentup.com
[System.Diagnostics.Process]::Start($documentupBaseUrl + "/recompile")
Write-Output "Sleeping for 5 second to wait for recompile of README.md at DocumentUp.com"
Start-Sleep -s 5
$wc = New-Object System.Net.WebClient
$wc.DownloadString($documentupBaseUrl) > .\index.html
git add index.html
git commit -m "Updated DocumentUp version of README.md"
git push

# Get out of the temporary repository location so we can remove it
Pop-Location

Remove-Item -Path $ghpagesRepoFolder -Force -Recurse
Write-Host "Done."
