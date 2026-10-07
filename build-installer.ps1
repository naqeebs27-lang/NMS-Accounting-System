param(
	[string]$Configuration = "Release",
	[string]$Runtime = "win-x64",
	[string]$ProjectPath = "AccountingSystem.App/AccountingSystem.App.csproj"
)

# Paths
$publishDir = Join-Path -Path (Get-Location) -ChildPath "publish\$Runtime"
$installerDir = Join-Path -Path (Get-Location) -ChildPath "installer"
$issPath = Join-Path -Path $installerDir -ChildPath "NaqeebsAccounting.iss"

Write-Host "Publishing project $ProjectPath -> $publishDir"

dotnet publish $ProjectPath -c $Configuration -r $Runtime /p:PublishSingleFile=true /p:SelfContained=true /p:IncludeNativeLibrariesForSelfExtract=true -o $publishDir
if ($LASTEXITCODE -ne 0) {
	Write-Error "dotnet publish failed"
	exit 1
}

# Ensure Resources folder and logo files are copied to the publish output so the installer includes them
$projectRoot = Split-Path -Path $ProjectPath -Parent
$resourcesDir = Join-Path -Path $projectRoot -ChildPath "Resources"
if (Test-Path $resourcesDir) {
	$targetResources = Join-Path -Path $publishDir -ChildPath "Resources"
	if (-not (Test-Path $targetResources)) {
		New-Item -ItemType Directory -Path $targetResources | Out-Null
	}

	$logos = @("nms.png", "NMS_logo.png")
	foreach ($logo in $logos) {
		$src = Join-Path -Path $resourcesDir -ChildPath $logo
		if (Test-Path $src) {
			Copy-Item -Path $src -Destination $targetResources -Force
			Write-Host "Copied resource $logo to publish Resources folder"
		}
	}
} else {
	Write-Host "No Resources folder found at $resourcesDir"
}

# Ensure installer directory exists
if (-not (Test-Path $installerDir)) {
	New-Item -ItemType Directory -Path $installerDir | Out-Null
}

# Check for Inno Setup compiler (ISCC.exe). GitHub Actions steps do not share
# process-local PATH changes, so also check the standard installation path.
$iscc = Get-Command iscc.exe -ErrorAction SilentlyContinue
if ($null -eq $iscc) {
	$standardIsccPath = Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"
	if (Test-Path $standardIsccPath) {
		$iscc = Get-Item $standardIsccPath
	}
}
if ($null -eq $iscc) {
	Write-Warning "Inno Setup compiler (ISCC.exe) was not found in PATH."
	Write-Host "Download and install Inno Setup: https://jrsoftware.org/isinfo.php"
	Write-Host "Then re-run this script. The installer script is at: $issPath"
	exit 0
}

Write-Host "Building installer using ISCC: $($iscc.Path)"
& $iscc.Path /O"$installerDir" /F"NaqeebsAccountingInstaller" "$issPath"
if ($LASTEXITCODE -ne 0) {
	Write-Error "ISCC failed to build installer"
	exit 1
}

Write-Host "Installer built. Output directory: $installerDir"
Write-Host "Open the .exe in the installer directory produced by Inno Setup."
