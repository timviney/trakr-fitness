<#
.SYNOPSIS
    Starts a local PostgreSQL Docker container for GymTracker development.

.DESCRIPTION
    This script starts a PostgreSQL container using Docker. The database password
    is read from the TRAKR_POSTGRES_PASSWORD environment variable. Optionally, you can
    provide a password parameter which will update the environment variable.

.PARAMETER Password
    Optional. If provided, sets/updates the TRAKR_POSTGRES_PASSWORD environment variable
    (User scope) and uses it for the container.

.EXAMPLE
    .\start-postgres.ps1
    # Uses existing TRAKR_POSTGRES_PASSWORD environment variable

.EXAMPLE
    .\start-postgres.ps1 -Password "mySecurePassword123"
    # Sets TRAKR_POSTGRES_PASSWORD env var and starts container with that password
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$Password
)

$ContainerName = "gymtracker-postgres"
$VolumeName = "gymtracker-postgres-data"
$PostgresPort = 5432
$PostgresImage = "postgres:16"

# Read settings from appsettings.Development.json
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appSettingsPath = Join-Path $scriptDir "GymTracker.Api\appsettings.Development.json"

if (-not (Test-Path $appSettingsPath)) {
    Write-Error "Could not find appsettings.Development.json at: $appSettingsPath"
    exit 1
}

$appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
$DatabaseName = $appSettings.PostgresSettings.Database
$Username = $appSettings.PostgresSettings.Username

if (-not $DatabaseName -or -not $Username) {
    Write-Error "PostgresSettings.Database and PostgresSettings.Username must be defined in appsettings.Development.json"
    exit 1
}

# Handle password
if ($Password) {
    # Set the environment variable for future sessions (User scope)
    [Environment]::SetEnvironmentVariable("TRAKR_POSTGRES_PASSWORD", $Password, "User")
    # Also set it for the current session
    $env:TRAKR_POSTGRES_PASSWORD = $Password
    Write-Host "TRAKR_POSTGRES_PASSWORD environment variable has been set." -ForegroundColor Green
}

$DbPassword = $env:TRAKR_POSTGRES_PASSWORD

if (-not $DbPassword) {
    Write-Error @"
TRAKR_POSTGRES_PASSWORD environment variable is not set.

To set it, either:
  1. Run this script with the -Password parameter:
     .\start-postgres.ps1 -Password "yourSecurePassword"

  2. Or set the environment variable manually:
     `$env:TRAKR_POSTGRES_PASSWORD = "yourSecurePassword"
     [Environment]::SetEnvironmentVariable("TRAKR_POSTGRES_PASSWORD", "yourSecurePassword", "User")

The password will be persisted in your user environment variables for future sessions.
"@
    exit 1
}

# Check if Docker is running
try {
    docker info 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker not responding"
    }
}
catch {
    Write-Error "Docker is not running. Please start Docker Desktop and try again."
    exit 1
}

# Check if container already exists
$existingContainer = docker ps -a --filter "name=$ContainerName" --format "{{.Names}}" 2>&1

if ($existingContainer -eq $ContainerName) {
    # Check if it's running
    $runningContainer = docker ps --filter "name=$ContainerName" --format "{{.Names}}" 2>&1
    
    if ($runningContainer -eq $ContainerName) {
        Write-Host "Container '$ContainerName' is already running." -ForegroundColor Yellow
        Write-Host "  Host: localhost"
        Write-Host "  Port: $PostgresPort"
        Write-Host "  Database: $DatabaseName"
        Write-Host "  Username: $Username"
        exit 0
    }
    else {
        Write-Host "Starting existing container '$ContainerName'..." -ForegroundColor Cyan
        docker start $ContainerName
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Container started successfully!" -ForegroundColor Green
        }
        exit $LASTEXITCODE
    }
}

# Create volume if it doesn't exist
$existingVolume = docker volume ls --filter "name=$VolumeName" --format "{{.Name}}" 2>&1
if ($existingVolume -ne $VolumeName) {
    Write-Host "Creating Docker volume '$VolumeName' for data persistence..." -ForegroundColor Cyan
    docker volume create $VolumeName
}

# Run new container
Write-Host "Starting new PostgreSQL container '$ContainerName'..." -ForegroundColor Cyan
Write-Host "  Image: $PostgresImage"
Write-Host "  Port: $PostgresPort"
Write-Host "  Database: $DatabaseName"
Write-Host "  Username: $Username"
Write-Host "  Volume: $VolumeName"

docker run -d `
    --name $ContainerName `
    -e POSTGRES_PASSWORD=$DbPassword `
    -e POSTGRES_USER=$Username `
    -e POSTGRES_DB=$DatabaseName `
    -p "${PostgresPort}:5432" `
    -v "${VolumeName}:/var/lib/postgresql/data" `
    $PostgresImage

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "PostgreSQL container started successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Connection details:" -ForegroundColor Cyan
    Write-Host "  Host:     localhost"
    Write-Host "  Port:     $PostgresPort"
    Write-Host "  Database: $DatabaseName"
    Write-Host "  Username: $Username"
    Write-Host ""
    Write-Host "To stop the container:  docker stop $ContainerName" -ForegroundColor Gray
    Write-Host "To remove the container: docker rm $ContainerName" -ForegroundColor Gray
    Write-Host "To remove data volume:  docker volume rm $VolumeName" -ForegroundColor Gray
}
else {
    Write-Error "Failed to start PostgreSQL container."
    exit 1
}
