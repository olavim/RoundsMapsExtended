& dotnet build

[xml]$config = Get-Content "Config.props"
$dir = $config.Project.PropertyGroup.RoundsFolder

& dotnet surity "$dir\Rounds.exe" -- --doorstop-enable true --doorstop-target "$PSScriptRoot\.bepinex\core\BepInEx.Preloader.dll"
