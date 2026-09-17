$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler is required.' }
$outputDirectory = Join-Path $projectRoot 'TestResults'
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$output = Join-Path $outputDirectory 'CoreTests.exe'
$sources = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets\Scripts\Core') -Filter '*.cs' | ForEach-Object { $_.FullName })
$sources += Join-Path $projectRoot 'Tests\CoreTests.cs'
& $compiler /nologo /target:exe "/out:$output" /r:System.Web.Extensions.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'Core compilation failed.' }
& $output (Join-Path $projectRoot 'Assets\Resources\Data\decisions.json') (Join-Path $projectRoot 'Assets\Resources\Data\shop.json') $outputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }
