$scriptPath = Split-Path $MyInvocation.MyCommand.Path -Parent
cd $scriptPath

$envPath = Join-Path $scriptPath .env

# Certain devs may have a slow internet connection
# therefore we'll pull the image BEFORE trying to start our container
# otherwise we risk the script breaking if we pull image while waiting on health check

Write-Output "Pulling mysql image..."
docker pull mysql:latest

docker-compose --env-file $envPath up -d social-coder-api-db

$check = $false
[int]$attempts = 10

while($attempts -gt 0 -and (-not $check))
{
    Write-Output "Waiting for DB Container to come online..."
    [string]$state = $(docker container inspect social-coder-api-db)
    $check = $state -match 'healthy'

    if($check)
    {
        break
    }
    
    $attempts = $attempts -= 1
    Write-Output "Waiting 5 seconds..."
    Start-Sleep 5
}

$serverPath = [IO.Path]::Combine($scriptPath, "SocialCoder.Web", "Server")
cd $serverPath

Write-Output "Running migrations..."
dotnet ef database update

Write-Output "Complete..."
cd $PSScriptRoot