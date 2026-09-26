param([Parameter(Mandatory=$true)][string]$Job)
$ErrorActionPreference='Stop'
$assetRoot=Split-Path -Parent $PSScriptRoot
$jobPath=(Resolve-Path -LiteralPath $Job).Path
if(-not $jobPath.StartsWith($assetRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Job must be in AssetProduction.'}
$spec=Get-Content -Raw -Encoding UTF8 -LiteralPath $jobPath | ConvertFrom-Json
$cliArguments=@($spec.arguments)
if($cliArguments.Count -lt 1){throw 'Empty Meshy job.'}
$receiptDir=Join-Path $assetRoot '02_Generated\receipts'
New-Item -ItemType Directory -Force -Path $receiptDir | Out-Null
$receipt=Join-Path $receiptDir ($spec.id+'.json')
if(($cliArguments -contains 'create') -and (Test-Path -LiteralPath $receipt)){throw 'Create job already has a receipt; reconcile it before submitting anything again.'}
if($cliArguments -contains 'create'){
    if(-not $spec.estimated_credits -or $spec.estimated_credits -le 0){throw 'A sourced cost estimate is required.'}
    $reserved=0
    Get-ChildItem -LiteralPath $receiptDir -Filter '*.json' | ForEach-Object {
        $previous=Get-Content -Raw -LiteralPath $_.FullName | ConvertFrom-Json
        if($previous.paid -and $previous.accepted -ne $false){$reserved += [int]$previous.estimated_credits}
    }
    # Initial runner is in CLI history before this wrapper existed.
    if($reserved+[int]$spec.estimated_credits+15 -gt 1100){throw 'User-authorized 1100-credit cap would be exceeded.'}
}
$ErrorActionPreference='Continue'
$raw=& meshy.cmd @cliArguments 2>&1 | Out-String
$cliExit=$LASTEXITCODE
$ErrorActionPreference='Stop'
$start=$raw.IndexOf('{')
$finish=$raw.LastIndexOf('}')
$response=$null
if($start -ge 0 -and $finish -gt $start){try{$response=$raw.Substring($start,$finish-$start+1)|ConvertFrom-Json}catch{}}
$paid=$cliArguments -contains 'create'
$accepted=$null
if($response -and $response.result.submission.state -eq 'accepted'){$accepted=$true}
if($response -and $response.error.http_status -ge 400 -and $response.error.http_status -lt 500){$accepted=$false}
$record=[ordered]@{id=$spec.id;paid=$paid;estimated_credits=$spec.estimated_credits;accepted=$accepted;cli_exit=$cliExit;response=$response;utc=[DateTime]::UtcNow.ToString('o')}
# A machine-written CLI receipt, not an authored source file.
$record|ConvertTo-Json -Depth 60|Set-Content -Encoding UTF8 -LiteralPath $receipt
$redacted=[regex]::Replace($raw,'(https?://[^\s"?]+)\?[^\s"]+','$1?[signed-query-redacted]')
Write-Output $redacted
if($cliExit -ne 0 -and -not ($response -and $response.ok -eq $true)){exit $cliExit}
