param([string]$UnityPath='C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe',[switch]$Character)
$ErrorActionPreference='Stop'
$productionRoot=Split-Path -Parent $PSScriptRoot
$workspaceRoot=Split-Path -Parent $productionRoot
$reviewRoot=Join-Path $workspaceRoot $(if($Character){'.utmp\runner-review'}else{'.utmp\asset-review'})
New-Item -ItemType Directory -Force -Path "$reviewRoot\Assets\Editor","$reviewRoot\Assets\Production","$reviewRoot\ProjectSettings","$reviewRoot\Packages" | Out-Null
$reviewSource=if($Character){'UnityRunnerReview.cs'}else{'UnityProductionReview.cs'}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot $reviewSource) -Destination "$reviewRoot\Assets\Editor\$reviewSource"
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'review-packages.json') -Destination "$reviewRoot\Packages\manifest.json"
Get-ChildItem -LiteralPath (Join-Path $productionRoot '04_Exports') -Directory | Where-Object { if($Character){$_.Name -eq 'CHAR_runner'}else{$_.Name -ne 'CHAR_runner'} } | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination "$reviewRoot\Assets\Production" -Recurse -Force
}
$env:SAM_ART_REPORT=Join-Path $productionRoot $(if($Character){'Reviews\Runner\Unity__v001__report.json'}else{'Reviews\Unity__v001__report.json'})
$env:SAM_ART_PACKAGE=Join-Path $productionRoot $(if($Character){'04_Exports\SAM_RunnerPilot__v001.unitypackage'}else{'04_Exports\SAM_ArtPilot__v001.unitypackage'})
$reviewLog=Join-Path $productionRoot $(if($Character){'Reviews\Runner\Unity__v001.log'}else{'Reviews\Unity__v001.log'})
$method=if($Character){'RunnerReview.Run'}else{'ProductionReview.Run'}
$arguments=@('-batchmode','-quit','-projectPath',('"'+$reviewRoot+'"'),'-executeMethod',$method,'-logFile',('"'+$reviewLog+'"'))
if(-not $Character){$arguments+='-nographics'}
$process=Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
if(-not $process.WaitForExit(600000)){throw 'Unity review exceeded ten minutes; inspect the log before retrying.'}
if($process.ExitCode -ne 0){throw "Unity review failed. See $reviewLog"}
Write-Host 'Isolated Unity model review and asset package export completed.'
