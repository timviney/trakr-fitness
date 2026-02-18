<#
.SYNOPSIS
    Starts a local PostgreSQL Docker container for GymTracker development.

.DESCRIPTION
    This script starts a PostgreSQL container using Docker. It will set a single environment
    variable `TRAKR_DB_CONNECTION_STRING_DEV` (User scope) containing the JDBC
    connection string for development. An optional -Password parameter can be
    provided to override the database password used for the container (default: "password").

.PARAMETER Password
    Optional. If provided, uses this as the PostgreSQL password for the container
    and places it into the generated connection string. The password will NOT be
    persisted as a separate TRAKR_POSTGRES_PASSWORD environment variable.
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

# Determine DB password to use for the container (default: 'password')
$DbPassword = if ($Password) { $Password } else { 'password' }

# Build the JDBC connection string and set it as a user-scoped environment variable
$ConnectionString = "jdbc:postgresql://localhost:$PostgresPort/$DatabaseName?user=$Username&password=$DbPassword"
[Environment]::SetEnvironmentVariable("TRAKR_DB_CONNECTION_STRING_DEV", $ConnectionString, "User")
$env:TRAKR_DB_CONNECTION_STRING_DEV = $ConnectionString
Write-Host "TRAKR_DB_CONNECTION_STRING_DEV environment variable has been set to:`n$ConnectionString" -ForegroundColor Green

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
