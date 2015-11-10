[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True,Position=1)]
  [string]$path,
  [Parameter(Position=2)]
  [AllowEmptyString()]
  [string[]]$types

)

foreach ($type in $types) {
    if ($type -Eq "") {
    } else {
        $type = "\" + $type;
    }

    $dir = "$path$type"

    $items = Get-ChildItem "$dir" -force| Where-Object {$_.name -match "-full.nupkg"} | Sort -Descending
    $first = $false;
    foreach ($item in $items) {
        if ($first -eq $false) {
            $first = $true;
        } else {
            Remove-Item "$dir\$item"
        }
    }

    $items = Get-ChildItem "$dir" -force| Where-Object {$_.name -match "-delta.nupkg"} | Sort -Descending
    $first = $false;
    foreach ($item in $items) {
        if ($first -eq $false) {
            $first = $true;
        } else {
            Remove-Item "$dir\$item"
        }
    }
}
