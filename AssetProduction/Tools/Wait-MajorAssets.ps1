param([ValidateSet('preview','refine')][string]$Stage='preview')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$workspace=Join-Path $root 'AssetProduction/02_Generated'
foreach($path in Get-ChildItem -LiteralPath (Join-Path $workspace 'Major') -Filter '*.json'){
 if($path.Name -eq 'history.json'){continue}
 $state=Get-Content -Raw -Encoding UTF8 -LiteralPath $path.FullName|ConvertFrom-Json
 if($state.id -eq 'palm'){continue}
 if(-not $state.$Stage -or -not $state.project){throw 'Missing task/project state'}
 Write-Host "Waiting $Stage $($state.id) task=$($state.$Stage)"
 $ErrorActionPreference='Continue'
 $raw=& meshy.cmd text-to-3d wait $state.$Stage --timeout 600 --project $state.project --stage $Stage --workspace $workspace --output-schema v1 --format json --no-update-check 2>&1 | Out-String
 $ErrorActionPreference='Stop'
 if($LASTEXITCODE -ne 0 -and $raw -notmatch '"ok":\s*true'){throw 'Wait incomplete; resume existing task, never create again.'}
 $raw | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $state.project ($Stage+'-wait.json'))
 if($Stage -eq 'preview' -and -not (Test-Path -LiteralPath (Join-Path $state.project 'geometry-preview.png'))){
   $ErrorActionPreference='Continue'
   $raw=& meshy.cmd download --resource text-to-3d --task-id $state.preview --asset thumbnail.primary --output (Join-Path $state.project 'geometry-preview.png') --project $state.project --stage preview --workspace $workspace --output-schema v1 --format json --no-update-check 2>&1 | Out-String
   $ErrorActionPreference='Stop'
   if(-not (Test-Path -LiteralPath (Join-Path $state.project 'geometry-preview.png')) -or $raw -notmatch '"ok":\s*true'){throw 'Preview download failed'}
 }
 Write-Host "Complete $Stage $($state.id)"
}
