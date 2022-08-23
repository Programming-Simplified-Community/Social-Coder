$envPath = Join-Path $PSScriptRoot .env
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

$serverPath = [IO.Path]::Combine($PSScriptRoot, "SocialCoder.Web", "Server")
cd $serverPath

Write-Output "Running migrations..."
dotnet ef database update

Write-Output "Complete..."
cd $PSScriptRoot