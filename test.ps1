dotnet build

[xml]$config = Get-Content "Config.props"
$dir = $config.Project.PropertyGroup.RoundsFolder

$testOutputFile = "test-output.txt"

New-Item "$dir\$testOutputFile" -Force | Out-Null

"Initializing test runner..."

& "$dir\Rounds.exe" -batchmode -nographics -test -testOutput "$dir\$testOutputFile"

Get-Content "$dir\$testOutputFile" -wait | ForEach-Object {
	if($_ -Match "\[PASS\]") { Write-Host $_ -ForegroundColor Green }
	elseif($_ -Match "\[FAIL\]") { Write-Host $_ -ForegroundColor Red }
	else { Write-Host $_ -ForegroundColor White }

	if($_ -Match "Tests failed:") { break }
}
