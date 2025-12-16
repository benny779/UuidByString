# NuGet Package Publishing Script
# This script builds and publishes the UuidByString package to NuGet.org

param(
    [string]$Configuration = "Release",
    [string]$EnvFile = ".env"
)

# Colors for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-ErrorMsg { Write-Host $args -ForegroundColor Red }

# Check if .env file exists
if (-not (Test-Path $EnvFile)) {
    Write-ErrorMsg "Error: $EnvFile file not found!"
    Write-Info "Please create a .env file with your NuGet API key."
    Write-Info "You can copy .env.example and add your API key."
    Write-Info "Get your API key from: https://www.nuget.org/account/apikeys"
    exit 1
}

# Load environment variables from .env file
Write-Info "Loading configuration from $EnvFile..."
Get-Content $EnvFile | ForEach-Object {
    if ($_ -match '^\s*([^#][^=]*?)\s*=\s*(.+?)\s*$') {
        $name = $matches[1]
        $value = $matches[2]
        Set-Item -Path "env:$name" -Value $value
    }
}

# Check if API key is set
if (-not $env:NUGET_API_KEY -or $env:NUGET_API_KEY -eq "your-api-key-here") {
    Write-ErrorMsg "Error: NUGET_API_KEY not set or still has default value!"
    Write-Info "Please update the .env file with your actual NuGet API key."
    exit 1
}

Write-Info "Starting NuGet package build and publish process..."

# Clean previous builds
Write-Info "Cleaning previous builds..."
dotnet clean UuidByString\UuidByString.csproj -c $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Clean failed!"
    exit $LASTEXITCODE
}

# extra cleanup to ensure no residual files
Get-ChildItem -Path ".\bin", ".\obj" -Recurse | Remove-Item -force -recurse

# Build and pack the project
Write-Info "Building and packing project..."
dotnet build UuidByString\UuidByString.csproj -c $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Build/Pack failed!"
    exit $LASTEXITCODE
}

# Find the generated package
$packagePath = Get-ChildItem -Path "UuidByString\bin\$Configuration" -Filter "*.nupkg" | Select-Object -First 1

if (-not $packagePath) {
    Write-ErrorMsg "Error: Package file not found!"
    exit 1
}

Write-Success "Package created: $($packagePath.FullName)"

# Ask for confirmation before publishing
Write-Warning "About to publish package to NuGet.org"
$confirmation = Read-Host "Continue? (y/n)"

if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Info "Publishing cancelled."
    exit 0
}

# Publish to NuGet
Write-Info "Publishing package to NuGet.org..."
dotnet nuget push $packagePath.FullName -k $env:NUGET_API_KEY -s https://api.nuget.org/v3/index.json

if ($LASTEXITCODE -eq 0) {
    Write-Success "Package published successfully!"
    Write-Info "It may take a few minutes for the package to appear on NuGet.org"
    Write-Info "Check status at: https://www.nuget.org/packages/UuidByString"
} else {
    Write-ErrorMsg "Publishing failed!"
    exit $LASTEXITCODE
}
