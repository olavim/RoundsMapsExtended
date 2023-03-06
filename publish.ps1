param(
	[Parameter(Mandatory)]
	[System.String]$Version,

	[Parameter(Mandatory)]
	[ValidateSet('Debug', 'Release')]
	[System.String]$Target,
	
	[Parameter(Mandatory)]
	[System.String]$TargetPath,
	
	[Parameter(Mandatory)]
	[System.String]$TargetAssembly,

	[Parameter(Mandatory)]
	[System.String]$RoundsPath,
	
	[Parameter(Mandatory)]
	[System.String]$SolutionPath,
	
	[Parameter(Mandatory)]
	[System.String]$ProjectPath
)

# Make sure Get-Location is the script path
Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Path)

# Test some preliminaries
("$TargetPath",
"$RoundsPath",
"$SolutionPath",
"$ProjectPath"
) | % {
	if (!(Test-Path "$_")) { Write-Error -ErrorAction Stop -Message "$_ folder is missing" }
}

# Go
Write-Host "Publishing for $Target from $TargetPath"

# Plugin name without ".dll"
$name = "$TargetAssembly" -Replace ('.dll')

# Create the mdb file
$pdb = "$TargetPath\$name.pdb"
if (Test-Path -Path "$pdb") {
	Write-Host "Create mdb file for plugin $name"
	Invoke-Expression "& `"$SolutionPath\libs\pdb2mdb.exe`" `"$TargetPath\$TargetAssembly`""
}

Write-Host ""
Write-Host "Updating local installation in $RoundsPath"

$plug = New-Item -Type Directory -Path "$RoundsPath\BepInEx\plugins\$name" -Force
Write-Host "  Copy $TargetAssembly to $plug"
Copy-Item -Path "$TargetPath\*" -Destination "$plug" -Force -Recurse

Write-Host ""

# Release packages for ThunderStore
if ($Target.Equals("Release") -and ($name.Equals("MapsExtended.Editor") -or $name.Equals("MapsExtended"))) {
	$package = "$SolutionPath\release"
	
	Write-Host "Packaging for ThunderStore"
	New-Item -Type Directory -Path "$package\Thunderstore" -Force
	$thunder = New-Item -Type Directory -Path "$package\Thunderstore\package"
	$thunder.CreateSubdirectory('plugins')
	Copy-Item -Path "$TargetPath\*" -Destination "$thunder\plugins\" -Recurse
	Copy-Item -Path "$SolutionPath\README.md" -Destination "$thunder\README.md"
	Copy-Item -Path "$ProjectPath\manifest.json" -Destination "$thunder\manifest.json"
	Copy-Item -Path "$ProjectPath\icon.png" -Destination "$thunder\icon.png"

	((Get-Content -path "$thunder\manifest.json" -Raw) -replace "#VERSION#", "$Version") | Set-Content -Path "$thunder\manifest.json"

	Remove-Item -Path "$package\Thunderstore\$name.$Version.zip" -Force
	Compress-Archive -Path "$thunder\*" -DestinationPath "$package\Thunderstore\$name.$Version.zip"
	$thunder.Delete($true)
}

# Release package for GitHub
if ($Target.Equals("Release") -and ($name.Equals("MapsExtended.Editor") -or $name.Equals("MapsExtended"))) {
	$package = "$SolutionPath\release"

	Write-Host "Packaging for GitHub"
	$pkg = New-Item -Type Directory -Path "$package\package"
	$pkg.CreateSubdirectory("BepInEx\plugins\$name")
	Copy-Item -Path "$TargetPath\*" -Destination "$pkg\BepInEx\plugins\$name" -Recurse

	Remove-Item -Path "$package\$name.$Version.zip" -Force
	Compress-Archive -Path "$pkg\*" -DestinationPath "$package\$name.$Version.zip"
	$pkg.Delete($true)
}

Pop-Location
