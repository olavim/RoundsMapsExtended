param
(
	[Parameter()]
	[switch]$Test,

	[Parameter()]
	[switch]$Suspend,

	[Parameter()]
	[switch]$NoBuild,

	[Parameter()]
	[System.String]$Port="55555"
)

Push-Location $PSScriptRoot

if (-not $NoBuild.IsPresent) {
	& dotnet build
}

[xml]$config = Get-Content "$PSScriptRoot\Config.props"
$roundsDir = $config.Project.PropertyGroup.RoundsDir
$bepinexDir = $config.Project.PropertyGroup.BepInExDir

$doorstopArgs = `
	"--doorstop-enable true", `
	"--doorstop-target-assembly `"$bepinexDir\core\BepInEx.Preloader.dll`"", `
	"--doorstop-mono-debug-enabled true", `
	"--doorstop-mono-debug-address `"127.0.0.1:$Port`"", `
	"--doorstop-mono-debug-suspend $Suspend";

try {
	switch ($Test.IsPresent)
	{
		$true {
			$args = @("surity ""$roundsDir\Rounds.exe"" --filter-stacktraces ""FluentAssertions.* | Surity.*"" --compact-stacktraces --") + $doorstopArgs
			$process = Start-Process "dotnet.exe" -PassThru -NoNewWindow -ArgumentList $args;
			break
		}
		default {
			$args = @("--") + $doorstopArgs
			$process = Start-Process "$roundsDir\Rounds.exe" -PassThru -NoNewWindow -ArgumentList $args;
			break
		}
	}

	Wait-Process -Id $process.Id
}
catch { 
	Write-Host "An error occurred."
}
finally {
	if (-not $process.HasExited) {
		Stop-Process -Id $process.Id
	}

	Pop-Location
}