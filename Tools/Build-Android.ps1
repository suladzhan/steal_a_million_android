param(
    [Parameter(Mandatory = $true)][string]$UnityPath,
    [ValidateSet('Apk', 'Aab')][string]$Format = 'Apk',
    [switch]$Release
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath $UnityPath)) { throw 'Unity.exe was not found at the supplied path.' }
$logDirectory = Join-Path $projectRoot 'Logs'
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
$method=if($Release -and $Format -eq 'Apk'){'ReleaseApk'}else{$Format}
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $projectRoot + '"'), '-buildTarget', 'Android', '-executeMethod', ('StealAMillion.Editor.AndroidBuild.' + $method), '-logFile', ('"' + (Join-Path $logDirectory 'android-build.log') + '"'))
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw 'Unity build failed. See Logs/android-build.log.' }
Write-Host "Build complete: Builds/Android/StealAMillion-V2.$($Format.ToLowerInvariant())"
