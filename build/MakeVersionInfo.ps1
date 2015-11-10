[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True,Position=1)]
  [string]$path,

  [Parameter(Mandatory=$True,Position=2)]
  [string]$releaseFolderName,

  [Parameter(Mandatory=$True,Position=3)]
  [string]$name
)

$obfPath = $path + "\bin\obf"
$destPath = $path + "\" + $releaseFolderName

# Get Version and Hash info
$filePath = $obfPath + "\" + $name + ".exe"
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($filePath).FileVersion
$md5 = new-object -TypeName System.Security.Cryptography.MD5CryptoServiceProvider
$hash = [System.BitConverter]::ToString($md5.ComputeHash([System.IO.File]::ReadAllBytes($filePath))).Replace("-", "").ToLower()
$versionInfo = $version + ":{D7F3EEAD-183C-47DE-BDC5-593539573F97}:" + $hash

# Create directory if doesnt exist yet
New-Item -ItemType Directory -Force -Path $destPath

# Write Version Hash info to file
[System.IO.File]::WriteAllLines(($destPath + "\versionInfo.txt"), $versionInfo)