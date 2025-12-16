# Version Bump Script
# Automatically increments the package version in UuidByString.csproj

param(
    [ValidateSet('major', 'minor', 'patch')]
    [string]$BumpType = "patch",
    
    [string]$ReleaseNotes = "",
    
    [string]$ProjectFile = "UuidByString\UuidByString.csproj"
)

function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-ErrorMsg { Write-Host $args -ForegroundColor Red }

# Check if project file exists
if (-not (Test-Path $ProjectFile)) {
    Write-ErrorMsg "Error: Project file not found at $ProjectFile"
    exit 1
}

# Read the project file
$content = Get-Content $ProjectFile -Raw

# Extract current version
if ($content -match '<PackageVersion>([\d\.]+)</PackageVersion>') {
    $currentVersion = $matches[1]
    Write-Info "Current version: $currentVersion"
} else {
    Write-ErrorMsg "Error: Could not find PackageVersion in project file"
    exit 1
}

# Parse version components
$versionParts = $currentVersion.Split('.')
if ($versionParts.Length -ne 3) {
    Write-ErrorMsg "Error: Version must be in format major.minor.patch (e.g., 1.0.0)"
    exit 1
}

$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
$patch = [int]$versionParts[2]

# Bump the version
switch ($BumpType) {
    'major' {
        $major++
        $minor = 0
        $patch = 0
    }
    'minor' {
        $minor++
        $patch = 0
    }
    'patch' {
        $patch++
    }
}

$newVersion = "$major.$minor.$patch"
Write-Info "New version: $newVersion"

# Update version in project file
$content = $content -replace '<PackageVersion>[\d\.]+</PackageVersion>', "<PackageVersion>$newVersion</PackageVersion>"

# Update release notes if provided
if ($ReleaseNotes) {
    $content = $content -replace '<PackageReleaseNotes>.*?</PackageReleaseNotes>', "<PackageReleaseNotes>$ReleaseNotes</PackageReleaseNotes>"
    Write-Info "Updated release notes"
}

# Write the updated content back
Set-Content -Path $ProjectFile -Value $content -NoNewline

Write-Success "Version bumped from $currentVersion to $newVersion"
Write-Info "Project file updated: $ProjectFile"
