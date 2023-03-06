& dotnet build

[xml]$config = Get-Content "Config.props"
$dir = $config.Project.PropertyGroup.RoundsFolder

& dotnet surity "$dir\Rounds.exe"
