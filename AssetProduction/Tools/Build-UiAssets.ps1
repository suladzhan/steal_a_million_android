param([switch]$ReplaceDrafts, [switch]$ReviewOnly)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -Path (Join-Path $PSScriptRoot 'BuildUiAssets.cs') -ReferencedAssemblies System.Drawing
$assetRoot = Split-Path -Parent $PSScriptRoot
if (-not $ReviewOnly) { [ProductionUi]::Build($assetRoot, [bool]$ReplaceDrafts) }
[ProductionUi]::Review($assetRoot)
if (Test-Path -LiteralPath (Join-Path $assetRoot 'Reviews\models__v001__manifest.json')) { [ProductionUi]::ReviewModels($assetRoot) }
