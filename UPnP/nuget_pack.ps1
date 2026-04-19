$project = "UPnP"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$projectPath = Join-Path $scriptDir "$project.csproj"

$versionStr = $env:APPVEYOR_REPO_TAG_NAME

if (-not [string]::IsNullOrEmpty($versionStr)) {
  $cleanVersion = $versionStr.TrimStart('v')
  
  Write-Host "Packing version $cleanVersion using dotnet pack..."
  Write-Host "Target project: $projectPath"

  & dotnet pack $projectPath -c Release -o . /p:Version=$cleanVersion --verbosity normal
  
  if ($LASTEXITCODE -ne 0) {
      Write-Error "dotnet pack failed with exit code $LASTEXITCODE"
      exit $LASTEXITCODE
  }
}
else {
  Write-Host "Version string is empty, skipping pack."
}
