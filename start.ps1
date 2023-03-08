[xml]$config = Get-Content "Config.props"
$dir = $config.Project.PropertyGroup.RoundsFolder

& "$dir\Rounds.exe" -- --doorstop-enable true --doorstop-target "$PSScriptRoot\.bepinex\core\BepInEx.Preloader.dll"
