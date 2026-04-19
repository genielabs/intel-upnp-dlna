$project = "UPnP"
$projectPath = (split-path -parent $MyInvocation.MyCommand.Definition) + "\$project\$project.csproj"
$versionStr = $env:APPVEYOR_REPO_TAG_NAME

if (-not [string]::IsNullOrEmpty($versionStr)) {
  # Rimuoviamo la 'v' iniziale se presente (es: v1.2.0 -> 1.2.0)
  $cleanVersion = $versionStr.TrimStart('v')
  
  Write-Host "Packing version $cleanVersion using dotnet pack..."

  # Passiamo la versione direttamente a dotnet pack senza toccare il file .csproj
  & dotnet pack $projectPath -c Release -o . /p:Version=$cleanVersion --verbosity normal
  
  if ($LASTEXITCODE -ne 0) {
      Write-Error "dotnet pack failed with exit code $LASTEXITCODE"
      exit $LASTEXITCODE
  }
}
else {
  Write-Host "Version string is empty, skipping pack."
}
