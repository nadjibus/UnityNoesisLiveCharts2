param(
    [string]$SourceRoot = (Join-Path $PSScriptRoot '../../LiveCharts2')
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$noesisAssembly = Join-Path $projectRoot 'Library/ScriptAssemblies/Noesis.NoesisGUI.dll'
$artifacts = Join-Path $projectRoot 'Logs/ChartBuild'
$chartProject = Join-Path $SourceRoot 'src/noesis/LiveChartsCore.Noesis/LiveChartsCore.Noesis.csproj'

if (-not (Test-Path -LiteralPath $noesisAssembly))
{
    throw 'Open Unity and let the Noesis package compile first.'
}

dotnet build $chartProject --framework netstandard2.1 --configuration Release --artifacts-path $artifacts "-p:NoesisUnityAssembly=$noesisAssembly"
if ($LASTEXITCODE -ne 0)
{
    throw 'Chart build failed.'
}

$output = Join-Path $artifacts 'bin/LiveChartsCore.Noesis/release_netstandard2.1'
$destination = Join-Path $projectRoot 'Assets/Plugins/LiveCharts2'
Copy-Item -LiteralPath (Join-Path $output 'LiveChartsCore.dll') -Destination $destination -Force
Copy-Item -LiteralPath (Join-Path $output 'LiveChartsCore.Noesis.dll') -Destination $destination -Force
