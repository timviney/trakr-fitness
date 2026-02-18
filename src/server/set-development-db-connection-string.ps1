<#
.SYNOPSIS
    Sets the DB connection string for development.

.DESCRIPTION
    This script starts a PostgreSQL container using Docker. The database password
    is read from the TRAKR_POSTGRES_PASSWORD environment variable. Optionally, you can
    provide a password parameter which will update the environment variable.

.PARAMETER ConnectionString
    Optional. If provided, sets/updates the TRAKR_POSTGRES_PASSWORD environment variable
    (User scope) and uses it for the container.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ConnectionString
)

# Trim surrounding whitespace first
$ConnectionString = $ConnectionString.Trim()

# If the string starts and ends with matching single or double quotes, remove them
if ($ConnectionString.Length -ge 2) {
    $first = $ConnectionString[0]
    $last = $ConnectionString[-1]
    if (($first -eq '"' -and $last -eq '"') -or ($first -eq "'" -and $last -eq "'")) {
        $ConnectionString = $ConnectionString.Substring(1, $ConnectionString.Length - 2)
    }
}

[Environment]::SetEnvironmentVariable("TRAKR_DB_CONNECTION_STRING_DEV", $ConnectionString, "User")
# Also set it for the current session
$env:TRAKR_DB_CONNECTION_STRING_DEV = $ConnectionString
Write-Host "TRAKR_DB_CONNECTION_STRING_DEV environment variable has been set." -ForegroundColor Green
