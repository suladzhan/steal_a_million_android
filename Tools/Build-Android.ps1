param(
    [Parameter(Mandatory = $true)][string]$UnityPath,
    [ValidateSet('Apk', 'Aab')][string]$Format = 'Apk'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath $UnityPath)) { throw 'Unity.exe was not found at the supplied path.' }
$logDirectory = Join-Path $projectRoot 'Logs'
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $projectRoot + '"'), '-buildTarget', 'Android', '-executeMethod', ('StealAMillion.Editor.AndroidBuild.' + $Format), '-logFile', ('"' + (Join-Path $logDirectory 'android-build.log') + '"'))
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw 'Unity build failed. See Logs/android-build.log.' }
Write-Host "Build complete: Builds/Android/StealAMillion.$($Format.ToLowerInvariant())"
