& dotnet build
if (-not $?) {
	throw 'Build failed'
}

[xml]$config = Get-Content "$PSScriptRoot\Config.props"
$roundsDir = $config.Project.PropertyGroup.RoundsDir
$bepinexDir = $config.Project.PropertyGroup.BepInExDir

& "$roundsDir\Rounds.exe" -- --doorstop-enable true --doorstop-target-assembly "$bepinexDir\core\BepInEx.Preloader.dll"
