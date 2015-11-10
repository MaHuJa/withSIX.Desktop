$src = "..\src\Tools"
$output = "Tools.7z"
Remove-Item("$output")
&".\7z.exe" -y a "$output" "$src"
