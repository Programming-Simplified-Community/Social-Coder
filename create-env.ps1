Param(
    [Parameter(Mandatory=$False)] [string] $databasePath,
    [Parameter(Mandatory=$False)] [string] $dbPassword,
    [Parameter(Mandatory=$False)] [string] $dbName,
    [Parameter(Mandatory=$False)] [string] $dbHost,
    [Parameter(Mandatory=$False)] [string] $jwtSecret,
    [Parameter(Mandatory=$False)] [string] $jwtAudience
)

$vars = @{}

# we are going to allow a developer to override default values
# default values are stored in the `.defaults` file
if($databasePath) { $vars["DB_PATH"] = $databasePath }
if($dbPassword) { $vars["DB_PASSWORD"] = $dbPassword }
if($dbName) { $vars["DB_NAME"] = $dbName }
if($dbHost) { $vars["DB_HOST"] = $dbHost }
if($jwtSecret) { $vars["JWT_SECRET"] = "$jwtSecret" }
if($jwtAudience) { $vars["JWT_AUDIENCE"] = "$jwtAudience" }

if(-not $databasePath)
{
    $databasePath = Join-Path $PSScriptRoot "Database"
}

$vars["DB_PATH"] = $databasePath

# similar to bash, we're 'importing' or 'sourcing' our scripts from our submodule... allowing us to call those functions!
$funcPath = [IO.Path]::Combine($PSScriptRoot, "Scripts", "funcs.ps1") 
. $funcPath

# pass in our dictionary of values
createEnv $vars -allowDefaults $True

# we need to load our env file so we know which values got used...
loadEnvFile

# create our connection string based on the values in our .env
$connectionEnvPath = Join-Path $PSScriptRoot ".connections"
$connectionString = "server=$([System.Environment]::GetEnvironmentVariable("DB_HOST"));user=root;password=$([System.Environment]::GetEnvironmentVariable("DB_PASSWORD"));database=$([System.Environment]::GetEnvironmentVariable("DB_NAME"))"
createEnv @{ DefaultConnection = "$connectionString"; } -saveTo $connectionEnvPath