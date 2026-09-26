param(
    [Parameter(Mandatory = $true)][string]$UnityPath,
    [switch]$Visual,
    [string]$TestFilter
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath $UnityPath)) { throw 'Unity.exe was not found at the supplied path.' }
$resultsDirectory = Join-Path $projectRoot 'TestResults'
New-Item -ItemType Directory -Force -Path $resultsDirectory | Out-Null
$reportName = if ($Visual) { 'unity-visual' } else { 'unity-editmode' }
$filter = if ($Visual) { 'StealAMillion.Tests.VisualFlowTests' } else { 'StealAMillion.Tests' }
if($TestFilter){$filter=$TestFilter}
$platform = if ($Visual) { 'PlayMode' } else { 'EditMode' }
$arguments = @('-batchmode', '-projectPath', ('"' + $projectRoot + '"'), '-runTests', '-testPlatform', $platform, '-testFilter', $filter, '-testResults', ('"' + (Join-Path $resultsDirectory ($reportName + '.xml')) + '"'), '-logFile', ('"' + (Join-Path $resultsDirectory ($reportName + '.log')) + '"'))
if (-not $Visual) { $arguments += '-nographics' }
$previousTestDirectory = $env:SAM_TEST_SAVE_DIRECTORY
try {
    $env:SAM_TEST_SAVE_DIRECTORY = Join-Path $resultsDirectory 'runtime-save'
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(300000)) {
        $process.Kill()
        throw 'Unity test run exceeded five minutes. See the test log.'
    }
} finally {
    $env:SAM_TEST_SAVE_DIRECTORY = $previousTestDirectory
}
if ($process.ExitCode -ne 0) { throw "Unity tests failed. See TestResults/$reportName.log." }
[xml]$report = Get-Content -LiteralPath (Join-Path $resultsDirectory ($reportName + '.xml'))
if ($report.'test-run'.result -ne 'Passed') { throw "Unity reported unsuccessful tests. See TestResults/$reportName.xml." }
Write-Host "Unity tests passed: $($report.'test-run'.passed) ($reportName)"
