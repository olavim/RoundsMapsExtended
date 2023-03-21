& dotnet build

[xml]$config = Get-Content "Config.props"
$roundsDir = $config.Project.PropertyGroup.RoundsDir
$bepinexDir = $config.Project.PropertyGroup.BepInExDir

& dotnet surity "$roundsDir\Rounds.exe" `
	--filter-stacktraces "FluentAssertions.* | Surity.*" `
	--compact-stacktraces `
	-- `
	--doorstop-enable true `
	--doorstop-target "$bepinexDir\core\BepInEx.Preloader.dll"
