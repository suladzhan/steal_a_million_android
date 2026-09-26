param(
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe',
    [string]$Method = 'StealAMillion.Editor.RunnerSetup.Configure',
    [string]$Log = 'runner-setup',
    [switch]$Graphics
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
New-Item -ItemType Directory -Force -Path (Join-Path $projectRoot 'TestResults') | Out-Null
$arguments = @('-batchmode', '-quit', '-projectPath', ('"' + $projectRoot + '"'), '-executeMethod', $Method, '-logFile', ('"' + (Join-Path $projectRoot "TestResults/$Log.log") + '"'))
if (-not $Graphics) { $arguments += '-nographics' }
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (-not $process.WaitForExit(600000)) { $process.Kill(); throw 'Unity exceeded ten minutes.' }
if ($process.ExitCode -ne 0) { throw "Unity failed. See TestResults/$Log.log" }
Write-Host "Unity completed: $Method"
