$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$fontDirectory = Join-Path $projectRoot 'Assets\Art\Fonts'
$rtlDirectory = Join-Path $projectRoot 'Assets\ThirdParty\RTLTMPro'
New-Item -ItemType Directory -Force -Path $fontDirectory, $rtlDirectory | Out-Null
$fonts = @('NotoSans/NotoSans-Bold.ttf', 'NotoSansArabic/NotoSansArabic-Regular.ttf', 'NotoSansDevanagari/NotoSansDevanagari-Regular.ttf', 'NotoSansThai/NotoSansThai-Regular.ttf')
foreach ($font in $fonts) {
    $destination = Join-Path $fontDirectory (Split-Path -Leaf $font)
    if (-not (Test-Path -LiteralPath $destination)) {
        Invoke-WebRequest -UseBasicParsing -Uri ('https://raw.githubusercontent.com/notofonts/noto-fonts/main/hinted/ttf/' + $font) -OutFile $destination
    }
}
$cjk = Join-Path $fontDirectory 'NotoSansCJKsc-Regular.otf'
if (-not (Test-Path -LiteralPath $cjk)) {
    Invoke-WebRequest -UseBasicParsing -Uri 'https://raw.githubusercontent.com/notofonts/noto-cjk/main/Sans/OTF/SimplifiedChinese/NotoSansCJKsc-Regular.otf' -OutFile $cjk
}
Invoke-WebRequest -UseBasicParsing -Uri 'https://raw.githubusercontent.com/notofonts/noto-fonts/main/LICENSE' -OutFile (Join-Path $fontDirectory 'Noto-LICENSE.txt')
Invoke-WebRequest -UseBasicParsing -Uri 'https://raw.githubusercontent.com/notofonts/noto-cjk/main/Sans/LICENSE' -OutFile (Join-Path $fontDirectory 'NotoCJK-LICENSE.txt')
$revision = 'f480419bbbffed1be3c129d68cc0182afcfbcac3'
$tree = Invoke-RestMethod -Uri ('https://api.github.com/repos/pnarimani/RTLTMPro/git/trees/' + $revision + '?recursive=1')
$runtime = 'Assets/RTLTMPro/Scripts/Runtime/'
foreach ($file in $tree.tree) {
    if ($file.path.StartsWith($runtime) -and $file.path.EndsWith('.cs') -and $file.path -notlike '*RTLTextMeshPro*') {
        $relative = $file.path.Substring($runtime.Length)
        $destination = Join-Path $rtlDirectory $relative
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
        Invoke-WebRequest -UseBasicParsing -Uri ('https://raw.githubusercontent.com/pnarimani/RTLTMPro/' + $tree.sha + '/' + $file.path) -OutFile $destination
    }
}
Invoke-WebRequest -UseBasicParsing -Uri ('https://raw.githubusercontent.com/pnarimani/RTLTMPro/' + $tree.sha + '/LICENSE') -OutFile (Join-Path $rtlDirectory 'LICENSE.txt')
Write-Host ('RTLTMPro source revision: ' + $tree.sha)
Get-ChildItem -LiteralPath $fontDirectory -File | Select-Object Name,Length
