param(
 [string]$UnityPath='C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe',
 [string]$BlenderPath='C:\Program Files\Blender Foundation\Blender 5.2\blender.exe',
 [switch]$SkipBuild
)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
 foreach($script in @('rebuild_major_geometry.py','rebake_major.py','report_major_geometry.py','render_major_store.py')){
  & $BlenderPath --background --python-exit-code 1 --python (Join-Path 'AssetProduction/Tools' $script)
  if($LASTEXITCODE -ne 0){throw "Blender step failed: $script"}
 }
 $import=Start-Process -FilePath $UnityPath -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$root+'"'),'-executeMethod','StealAMillion.Editor.MajorArtSetup.BuildAll','-logFile','TestResults/major-art-import.log') -WindowStyle Hidden -PassThru
 $import.WaitForExit();if($import.ExitCode -ne 0){throw 'Major Unity import failed'}
 & "$PSScriptRoot/Test-Unity.ps1" -UnityPath $UnityPath -Visual
 & "$PSScriptRoot/Test-Unity.ps1" -UnityPath $UnityPath
 if(-not $SkipBuild){
  & "$PSScriptRoot/Build-Android.ps1" -UnityPath $UnityPath -Release
  & "$PSScriptRoot/Verify-Android.ps1" -UnityPath $UnityPath -Release
 }
} finally {Pop-Location}
