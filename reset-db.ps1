$scriptPath = Split-Path $MyInvocation.MyCommand.Path -Parent
cd $scriptPath

$envPath = Join-Path $scriptPath .env

Write-Output "Tearing down DB container"
docker-compose down

# read first line of our env file (which is our database path)
$firstLine = Get-Content $envPath -First 1
$dbPath = $firstLine.Split('=')[1]

write-output $dbPath

if(Test-Path $dbPath)
{
    Write-Output "Cleaning up old database files at: $dbPath"
    Remove-Item -Recurse $dbPath
}

# reuse our start-db script
./start-db.ps1