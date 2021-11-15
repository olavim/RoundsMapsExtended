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

if ($name.Equals("MapsExtended") -or $name.Equals("MapsExtended.Editor")) {
	Write-Host "Updating local installation in $RoundsPath"
	
	$plug = New-Item -Type Directory -Path "$RoundsPath\BepInEx\plugins\$name" -Force
	Write-Host "Copy $TargetAssembly to $plug"
	Copy-Item -Path "$TargetPath\$name.dll" -Destination "$plug" -Force

	if ($name.Equals("MapsExtended.Editor")) {
		Copy-Item -Path "$TargetPath\$name.UI.dll" -Destination "$plug" -Force
		Copy-Item -Path "$TargetPath\NetTopologySuite.dll" -Destination "$plug" -Force
		Copy-Item -Path "$TargetPath\System.Buffers.dll" -Destination "$plug" -Force
	}
}

# Release packages for ThunderStore
if ($Target.Equals("Release") -and ($name.Equals("MapsExtended.Editor") -or $name.Equals("MapsExtended"))) {
	$package = "$SolutionPath\release"
	
	Write-Host "Packaging for ThunderStore"
	New-Item -Type Directory -Path "$package\Thunderstore" -Force
	$thunder = New-Item -Type Directory -Path "$package\Thunderstore\package"
	$thunder.CreateSubdirectory('plugins')
	Copy-Item -Path "$TargetPath\$name.dll" -Destination "$thunder\plugins\"
	Copy-Item -Path "$SolutionPath\README.md" -Destination "$thunder\README.md"
	Copy-Item -Path "$ProjectPath\manifest.json" -Destination "$thunder\manifest.json"
	Copy-Item -Path "$ProjectPath\icon.png" -Destination "$thunder\icon.png"

	if ($name.Equals("MapsExtended.Editor")) {
		Copy-Item -Path "$TargetPath\$name.UI.dll" -Destination "$thunder\plugins\"
		Copy-Item -Path "$TargetPath\NetTopologySuite.dll" -Destination "$thunder\plugins\"
		Copy-Item -Path "$TargetPath\System.Buffers.dll" -Destination "$thunder\plugins\"
	}

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
	$pkg.CreateSubdirectory('BepInEx\plugins')
	Copy-Item -Path "$TargetPath\$name.dll" -Destination "$pkg\BepInEx\plugins\$name.dll"

	if ($name.Equals("MapsExtended.Editor")) {
		Copy-Item -Path "$TargetPath\$name.UI.dll" -Destination "$pkg\BepInEx\plugins\"
		Copy-Item -Path "$TargetPath\NetTopologySuite.dll" -Destination "$pkg\BepInEx\plugins\"
		Copy-Item -Path "$TargetPath\System.Buffers.dll" -Destination "$pkg\BepInEx\plugins\"
	}

	Remove-Item -Path "$package\$name.$Version.zip" -Force
	Compress-Archive -Path "$pkg\*" -DestinationPath "$package\$name.$Version.zip"
	$pkg.Delete($true)
}

Pop-Location
