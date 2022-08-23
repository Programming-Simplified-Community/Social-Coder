Param(
    [Parameter(Mandatory=$false)] [string] $databasePath,
    [Parameter(Mandatory=$false)] [string] $dbPassword,
    [Parameter(Mandatory=$false)] [string] $dbName,
    [Parameter(Mandatory=$false)] [string] $dbHost
)

$basePath = $PSScriptRoot
$envPath = Join-Path $PSScriptRoot ".env"

if(Test-Path $envPath)
{
    Remove-Item $envPath
}

# if the database path has not been set
if(-not($databasePath))
{
    $databasePath = Get-Location
    Write-Output "Defaulting Database Path to $databasePath"
}

if(-not($dbPassword))
{
    $dbPassword = "my-awesome-password"
    Write-Output "Using Default Password"
}

if(-not($dbName))
{
    $dbName = "social-coder"
    Write-Output "Using Default DB Name"
}

if(-not($dbHost)) 
{ 
    $dbHost = "localhost"
    Write-Output "Using Default Host"
}


$settingsPath = [IO.Path]::Combine($PSScriptRoot, "SocialCoder.Web", "Server", "appsettings.json")
Write-Output "appsettings.json path: $settingsPath"
$settings = Get-Content $settingsPath

$connectionString = "server=$dbHost;user=root;password=$dbPassword;database=$dbName"
$settings = $settings -replace ('"DefaultConnection": "(.*?)"', """DefaultConnection"": ""$connectionString""")

write-output $settings

Write-Output "Updating connection string..."
Set-Content $settingsPath $settings

New-Item $envPath
Add-Content $envPath "DB_PATH=$databasePath\Database"
Add-Content $envPath "DB_PASSWORD=$dbPassword"
Add-Content $envPath "DB_HOST=$dbHost"
Add-Content $envPath "DB_NAME=$dbName"