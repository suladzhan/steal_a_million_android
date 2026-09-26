param([ValidateSet('preview','refine','download')][string]$Stage='preview')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$workspace=Join-Path $root 'AssetProduction/02_Generated'
$folder=Join-Path $workspace 'Major'
$jobs=Join-Path $root 'AssetProduction/Tools/jobs/Major'
New-Item -ItemType Directory -Force -Path $folder,$jobs | Out-Null
$assets=@(
 @{id='townhouse';subject='complete two storey corner townhouse storefront with broad windows, rounded roof, striped awning and doorstep, warm cream stucco and coral roof, no signs'},
 @{id='tower';subject='complete eight storey modern city office tower with rounded corners, navy window strips, pale blue facade, stepped rooftop and broad entrance, no text'},
 @{id='villa';subject='complete luxury coastal villa, rounded ivory walls, turquoise windows, coral tile roof, terrace and arched entrance, no plants'},
 @{id='future';subject='complete futuristic elegant tower with rounded white shell, cyan window bands, sculpted tapered rooftop and antenna, no signs'},
 @{id='palm2';subject='one single cartoon coconut palm, tall bare curved brown trunk, leaves ONLY at the very top, exactly six long simple smooth green fronds in one umbrella crown, NO banana leaves, NO foliage around trunk, no ground or pot'},
 @{id='tree';subject='one complete stylized round broadleaf tree with thick tan trunk, three large fluffy green foliage clusters, no ground or pot'},
 @{id='car';subject='one complete cute compact red hatchback car, rounded beveled toy body, navy opaque windows, four black wheels, white headlights, facing forward, no logos'},
 @{id='barrier';subject='one complete portable traffic barricade, broad coral red rectangular panel with four diagonal cream stripes, rounded navy support feet, no text'},
 @{id='female';subject='complete adult female stylized cartoon mobile game runner, friendly expressive face, short tidy brown ponytail, royal blue simple fitted T-shirt, navy trousers, white sneakers, modest athletic outfit, hands relaxed, full body, separated limbs in A pose, no props'}
)
function Invoke-MeshyResponse([string[]]$Arguments){
 $ErrorActionPreference='Continue'
 $raw=& meshy.cmd @Arguments 2>&1 | Out-String
 $ErrorActionPreference='Stop'
 if($LASTEXITCODE -ne 0 -and $raw -notmatch '"ok":\s*true'){throw "Meshy step failed; reconcile existing task. $raw"}
 $start=$raw.IndexOf('{');$end=$raw.LastIndexOf('}')
 if($start -lt 0){throw 'No Meshy response'}
 $response=$raw.Substring($start,$end-$start+1)|ConvertFrom-Json
 if(-not $response.ok){throw 'Meshy returned an unsuccessful response'}
 return $response
}
foreach($asset in $assets){
 $statePath=Join-Path $folder ($asset.id+'.json')
 if(Test-Path -LiteralPath $statePath){$state=Get-Content -Raw -Encoding UTF8 -LiteralPath $statePath|ConvertFrom-Json}else{$state=[pscustomobject]@{id=$asset.id;preview=$null;refine=$null;project=$null}}
 if($Stage -ne 'download'){
   if(-not $state.$Stage){
     $argsList=@('text-to-3d','create','--mode',$Stage,'--target-formats','glb','--async')
     if($Stage -eq 'preview'){$prompt="Premium original stylized mobile runner game asset: $($asset.subject). Soft rounded forms, chunky readable silhouette, clean toy proportions, complete isolated subject, no scene, no logos, no text.";$argsList+=@('--prompt',$prompt);if($asset.id -eq 'female'){$argsList+=@('--pose-mode','a-pose')}}
     else{if(-not $state.preview -or -not $state.project){throw 'Preview state missing'};$texture="Premium cartoon painted materials for $($asset.subject). Saturated but gentle colours, smooth clean surface, no dirt, no baked shadows, no logos, no text.";$argsList+=@('--preview-task-id',$state.preview,'--enable-pbr','false','--texture-resolution','2k','--remove-lighting','true','--texture-prompt',$texture,'--project',$state.project,'--stage','refine')}
     $argsList+=@('--workspace',$workspace,'--output-schema','v1','--format','json','--no-update-check')
     $jobId='major-'+$asset.id+'-'+$Stage
     $jobPath=Join-Path $jobs ($jobId+'.json')
     @{id=$jobId;estimated_credits=$(if($Stage -eq 'preview'){20}else{10});arguments=$argsList}|ConvertTo-Json -Depth 10|Set-Content -Encoding UTF8 -LiteralPath $jobPath
     & (Join-Path $PSScriptRoot 'Invoke-Meshy.ps1') -Job $jobPath | Out-Null
     if($LASTEXITCODE -ne 0){throw 'Submission failed; do not resubmit'}
     $receipt=Get-Content -Raw -Encoding UTF8 (Join-Path $workspace ('receipts/'+$jobId+'.json'))|ConvertFrom-Json
     $state.$Stage=$receipt.response.result.submission.task_id
     if(-not $state.$Stage){throw 'Accepted task ID missing; reconcile receipt'}
     $state|ConvertTo-Json|Set-Content -Encoding UTF8 -LiteralPath $statePath
   }
   if(-not $state.project){$init=Invoke-MeshyResponse @('project','init','--root',$folder,'--name',$asset.id,'--task-id',$state.preview,'--task-type','text-to-3d','--workspace',$workspace,'--output-schema','v1','--format','json','--no-update-check');$state.project=$init.result.project_dir;$state|ConvertTo-Json|Set-Content -Encoding UTF8 -LiteralPath $statePath}
   Write-Host "$Stage $($asset.id): $($state.$Stage) project=$($state.project)"
 }else{
   if(-not $state.refine){throw 'Refine state missing'}
   foreach($selection in @(@('model-format','glb','model.glb'),@('asset','thumbnail.primary','preview.png'))){
     $output=Join-Path $state.project $selection[2]
     if(-not (Test-Path -LiteralPath $output)){$null=Invoke-MeshyResponse @('download','--resource','text-to-3d','--task-id',$state.refine,('--'+$selection[0]),$selection[1],'--output',$output,'--project',$state.project,'--stage','delivered','--workspace',$workspace,'--output-schema','v1','--format','json','--no-update-check')}
   }
   Write-Host "Delivered $($asset.id): $($state.project)"
 }
}
