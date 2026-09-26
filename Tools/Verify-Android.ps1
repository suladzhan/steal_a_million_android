param(
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe',
    [string]$Apk = 'Builds\Android\StealAMillion-V2.apk',
    [switch]$Release
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$apkPath = (Resolve-Path -LiteralPath (Join-Path $root $Apk)).Path
$android = Join-Path (Split-Path -Parent $UnityPath) 'Data\PlaybackEngines\AndroidPlayer'
$buildTools = Join-Path $android 'SDK\build-tools\36.0.0'
$env:JAVA_HOME = Join-Path $android 'OpenJDK'
& (Join-Path $buildTools 'apksigner.bat') verify --verbose --print-certs $apkPath
if ($LASTEXITCODE -ne 0) { throw 'APK signature verification failed.' }
& (Join-Path $buildTools 'zipalign.exe') -c -P 16 4 $apkPath
if ($LASTEXITCODE -ne 0) { throw 'APK zip alignment verification failed.' }
$badging = & (Join-Path $buildTools 'aapt.exe') dump badging $apkPath
if ($LASTEXITCODE -ne 0) { throw 'APK manifest inspection failed.' }
$badging | Select-String 'package:|sdkVersion:|targetSdkVersion:|native-code:|uses-permission:'
if ($badging -match 'android.permission.ACCESS_(FINE|COARSE)_LOCATION') { throw 'Unexpected GPS permission.' }
if (-not ($badging -match "native-code: 'arm64-v8a'")) { throw 'Expected ARM64 APK.' }
if($Release){
    if(-not ($badging -match "targetSdkVersion:'36'")){throw 'Release must target API 36.'}
    $manifest=& (Join-Path $buildTools 'aapt.exe') dump xmltree $apkPath AndroidManifest.xml
    if($LASTEXITCODE -ne 0){throw 'Manifest inspection failed.'}
    if($manifest -match 'android:debuggable.*0xffffffff'){throw 'Release APK is debuggable.'}
    Write-Host 'RELEASE_MANIFEST_PASS: API 36, non-debuggable'
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($apkPath)
$verifyDirectory = Join-Path $root 'TestResults\AndroidNative'
New-Item -ItemType Directory -Force -Path $verifyDirectory | Out-Null
try {
    foreach ($entry in $archive.Entries | Where-Object { $_.FullName.StartsWith('lib/arm64-v8a/') -and $_.Name.EndsWith('.so') }) {
        $file = Join-Path $verifyDirectory ([IO.Path]::GetFileName($entry.Name))
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $file, $true)
        $headers = & (Join-Path $android 'NDK\toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-readelf.exe') -lW $file
        if ($LASTEXITCODE -ne 0) { throw "Unable to inspect $($entry.Name)." }
        $loads = @($headers | Where-Object { $_ -match '^\s*LOAD\s' })
        if ($loads.Count -eq 0) { throw "No ELF load segments in $($entry.Name)." }
        foreach ($line in $loads) {
            $alignment = ($line.Trim() -split '\s+')[-1]
            if ([Convert]::ToInt64($alignment, 16) -lt 16384) { throw "ELF alignment below 16 KB in $($entry.Name)." }
        }
        Write-Host "ELF 16 KB alignment passed: $($entry.Name)"
    }
} finally { $archive.Dispose() }
Get-Item -LiteralPath $apkPath | Select-Object Name,Length
Get-FileHash -LiteralPath $apkPath -Algorithm SHA256
