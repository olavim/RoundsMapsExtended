param(
	[Parameter(Mandatory)]
	[System.String]$Version,
	
	[Parameter(Mandatory)]
	[System.String]$TargetPath,
	
	[Parameter(Mandatory)]
	[System.String]$TargetAssembly,
	
	[Parameter(Mandatory)]
	[System.String]$SolutionPath,
	
	[Parameter(Mandatory)]
	[System.String]$ProjectPath
)

# Make sure Get-Location is the script path
Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Path)

# Test some preliminaries
("$TargetPath",
"$SolutionPath",
"$ProjectPath"
) | % {
	if (!(Test-Path "$_")) { Write-Error -ErrorAction Stop -Message "$_ folder is missing" }
}

# Plugin name without ".dll"
$name = "$TargetAssembly" -Replace ('.dll')
$releaseDir = "$SolutionPath\release"

# Release packages for ThunderStore
$baseDir = "$releaseDir\thunderstore"
$tempDir = "$baseDir\temp"

Write-Host "Packaging $name-$Version for ThunderStore"

New-Item -Type Directory -Path $tempDir -Force | Out-Null
New-Item -Type Directory -Path "$tempDir\plugins" -Force | Out-Null

Copy-Item -Path "$TargetPath\*" -Destination "$tempDir\plugins\" -Recurse
Copy-Item -Path "$SolutionPath\README.md" -Destination "$tempDir\README.md"
Copy-Item -Path "$ProjectPath\manifest.json" -Destination "$tempDir\manifest.json"
Copy-Item -Path "$ProjectPath\icon.png" -Destination "$tempDir\icon.png"

((Get-Content -path "$tempDir\manifest.json" -Raw) -replace "#VERSION#", "$Version") | Set-Content -Path "$tempDir\manifest.json"

$zipFile = "$baseDir\$name.$Version.zip"
if (Test-Path $zipFile) {
	Remove-Item -Path $zipFile
}

Compress-Archive -Path "$tempDir\*" -DestinationPath "$baseDir\$name.$Version.zip"

Remove-Item -Path $tempDir -Recurse

# Release package for GitHub
$baseDir = "$releaseDir\plugin"
$tempDir = "$baseDir\temp"

Write-Host "Packaging $name-$Version for GitHub"

New-Item -Type Directory -Path $tempDir -Force | Out-Null
New-Item -Type Directory -Path "$tempDir\BepInEx\plugins\$name" -Force | Out-Null

Copy-Item -Path "$TargetPath\*" -Destination "$tempDir\BepInEx\plugins\$name" -Recurse

$zipFile = "$baseDir\$name.$Version.zip"
if (Test-Path $zipFile) {
	Remove-Item -Path $zipFile
}

Compress-Archive -Path "$tempDir\*" -DestinationPath $zipFile
Remove-Item -Path $tempDir -Recurse

Pop-Location
