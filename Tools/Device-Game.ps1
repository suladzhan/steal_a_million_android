param(
    [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z0-9._:-]+$')][string]$Serial,
    [Parameter(Mandatory = $true)][ValidateSet('Info','Backup','Install','Launch','Capture','Logs','Tap','Swipe','Back','Home','Stop','Memory','Frames','EnableProfile','Performance')][string]$Action,
    [int]$X = 0, [int]$Y = 0, [int]$EndX = 0, [int]$EndY = 0, [int]$Duration = 350,
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$adb = Join-Path (Split-Path -Parent $UnityPath) 'Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe'
$package = 'com.sulik.stealamillion'
$remote = "/sdcard/Android/data/$package/files"
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$output = Join-Path $root "TestResults\Device\$stamp-$Action"
function Invoke-Adb {
    & $adb -s $Serial @args
    if ($LASTEXITCODE -ne 0) { throw "ADB command failed: $($args -join ' ')" }
}
function Assert-GameForeground {
    $resumed = Invoke-Adb shell dumpsys activity activities | Select-String '(topResumedActivity|mResumedActivity)'
    if (-not ($resumed -match [regex]::Escape($package))) { throw 'Game is not foreground. Refusing to capture or send touch input to another app.' }
}
switch ($Action) {
    EnableProfile {
        New-Item -ItemType Directory -Force -Path $output | Out-Null
        $flag=Join-Path $output 'enable-profile';Set-Content -LiteralPath $flag -Value 'local QA only' -Encoding ASCII
        Invoke-Adb shell mkdir -p "$remote/QA"
        Invoke-Adb push $flag "$remote/QA/enable-profile"
    }
    Performance {
        New-Item -ItemType Directory -Force -Path $output | Out-Null
        Invoke-Adb pull "$remote/QA/performance-latest.json" (Join-Path $output 'performance-latest.json')
        Get-Content -LiteralPath (Join-Path $output 'performance-latest.json')
    }
    Info {
        Invoke-Adb shell getprop ro.product.model
        Invoke-Adb shell getprop ro.build.version.release
        Invoke-Adb shell getprop ro.product.cpu.abilist
        Invoke-Adb shell wm size
        Invoke-Adb shell dumpsys package $package | Select-String 'versionCode=|versionName=|primaryCpuAbi='
    }
    Backup {
        if ((Invoke-Adb get-state).Trim() -ne 'device') { throw 'Device is not available for backup.' }
        New-Item -ItemType Directory -Path $output -Force | Out-Null
        foreach ($name in @('progress.sav','progress.sav.bak','progress.sav.v1','progress.sav.v1.bak')) {
            & $adb -s $Serial shell test -f "$remote/$name"
            if ($LASTEXITCODE -eq 0) { Invoke-Adb pull "$remote/$name" (Join-Path $output $name) }
        }
        if (@(Get-ChildItem -LiteralPath $output -File).Count -eq 0) { throw 'No save files were backed up. Check the device connection and game data.' }
        Get-ChildItem -LiteralPath $output -File | Get-FileHash -Algorithm SHA256
        Write-Host "Backup: $output"
    }
    Install { Invoke-Adb install -r (Join-Path $root 'Builds\Android\StealAMillion-V2.apk') }
    Launch { Invoke-Adb shell am start -W -n "$package/com.unity3d.player.UnityPlayerGameActivity" }
    Capture {
        Assert-GameForeground
        New-Item -ItemType Directory -Path $output -Force | Out-Null
        Invoke-Adb shell screencap -p "$remote/qa-screenshot.png"
        $screen = Join-Path $output 'screen.png'
        Invoke-Adb pull "$remote/qa-screenshot.png" $screen
        Add-Type -AssemblyName System.Drawing
        $bitmap = [System.Drawing.Bitmap]::new($screen)
        try {
            $magenta = 0; $samples = 0
            $colors = [System.Collections.Generic.HashSet[int]]::new()
            for ($py = 0; $py -lt $bitmap.Height; $py += 4) {
                for ($px = 0; $px -lt $bitmap.Width; $px += 4) {
                    $pixel = $bitmap.GetPixel($px, $py)
                    if ($pixel.R -gt 240 -and $pixel.G -lt 25 -and $pixel.B -gt 240) { $magenta++ }
                    [void]$colors.Add($pixel.ToArgb()); $samples++
                }
            }
            $metrics = [ordered]@{ width = $bitmap.Width; height = $bitmap.Height; sampledColors = $colors.Count; errorMagentaFraction = $magenta / $samples }
            $metrics | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'pixels.json') -Encoding UTF8
            $metrics
            if ($magenta / $samples -gt 0.002) { Write-Warning 'Possible missing shader: magenta pixels detected. Inspect screenshot.' }
            if ($colors.Count -lt 20) { Write-Warning 'Possible blank render. Inspect screenshot.' }
        } finally { $bitmap.Dispose() }
        Write-Host "Screenshot: $output\screen.png"
    }
    Logs {
        $processId = (Invoke-Adb shell pidof $package).Trim()
        if ($processId -notmatch '^\d+$') { throw 'Expected one running game process.' }
        New-Item -ItemType Directory -Path $output -Force | Out-Null
        $lines = Invoke-Adb logcat -d --pid=$processId -v threadtime
        $lines | Set-Content -LiteralPath (Join-Path $output 'logcat.txt') -Encoding UTF8
        $issues = $lines | Select-String '\s[EF]\s+Unity\s*:|FATAL EXCEPTION|Fatal signal|\s[WE]\s+Unity\s*:.*(shader|glyph|font|save)'
        $issues
        Write-Host "Game error/warning lines: $(@($issues).Count)"
        Write-Host "Game log: $output\logcat.txt"
    }
    Tap { Assert-GameForeground; Invoke-Adb shell input tap $X $Y }
    Swipe { Assert-GameForeground; Invoke-Adb shell input swipe $X $Y $EndX $EndY $Duration }
    Back { Assert-GameForeground; Invoke-Adb shell input keyevent 4 }
    Home { Assert-GameForeground; Invoke-Adb shell input keyevent 3 }
    Stop { Invoke-Adb shell am force-stop $package }
    Memory { Invoke-Adb shell dumpsys meminfo $package }
    Frames { Invoke-Adb shell dumpsys gfxinfo $package framestats }
}
