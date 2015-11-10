[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True,Position=1)]
  [string]$path,
  [Parameter(Mandatory=$True,Position=2)]
  [string]$prefix
  )

Move-Item -Force "$path\Setup.exe" "$path\$prefix-Setup.exe"