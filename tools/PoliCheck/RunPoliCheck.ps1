
param(
  [string]$BuildSourceDir,
  [string]$folderName,
  [string]$branchName,
  [string]$resultRoot,
  [string]$PoliCheckPath
)

#
#Example: 
# RunPoliCheck.ps1 -BuildSourceDir "C:\BuildAgent\_work\32\s" 
#                  -folderName "src" 
#                  -branchName "odata.net-master" 
#                  -resultRoot "C:\Users\ODatabld\Documents\PoliCheck\LatestRunResult" 
#                  -PoliCheckPath "C:\Program Files (x86)\Microsoft\PoliCheck\"
#

$targetPath= "${BuildSourceDir}\${folderName}"
Write-Output "targetPath: ${targetPath}"
$result="${resultRoot}\${branchName}\poli_result_${folderName}.xml"

cd "${PoliCheckPath}"

.\Policheck.exe /F:$targetPath /T:9 /Sev:"1|2" /PE:2 /O:$result

$FileContent = Get-Content $result
$PassResult = Select-String -InputObject $FileContent -Pattern "<TermTbl />"

If ($PassResult.Matches.Count -eq 0) {
  Write-Error "PoliCheck failed for target ${targetPath}. For details, please check this result file on build machine: ${result}: section <Result TotalOccurences=...>."
  exit 1
}

Write-Output "PoliCheck pass for target ${targetPath}"