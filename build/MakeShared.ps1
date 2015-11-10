$src = "shared"
mkdir -Force "$src"

# LOCAL
Copy-Item -Force "..\src\SN.withSIX.SelfUpdater.Presentation.Wpf\bin\release\obf\withSIX-SelfUpdater.exe" "$src\"
Copy-Item -Force "..\src\SN.withSIX.Updater.Presentation.Wpf\bin\release\obf\withSIX-Updater.exe" "$src\"

# TFS Build
Copy-Item -Force "..\..\bin\obf\withSIX-SelfUpdater.exe" "$src\"
Copy-Item -Force "..\..\bin\obf\withSIX-Updater.exe" "$src\"

$output = "Shared.7z"
Remove-Item -Force "$output"
&".\7z.exe" -y a "$output" ".\$src\*.*"
