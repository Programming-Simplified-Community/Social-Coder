$scriptPath = Split-Path $MyInvocation.MyCommand.Path -Parent
cd $scriptPath

# similar to bash, we're 'importing' or 'sourcing' our scripts from our submodule... allowing us to call those functions!
$funcPath = [IO.Path]::Combine($PSScriptRoot, "Scripts", "funcs.ps1")
. $funcPath

startDb -composeFolder $PSScriptRoot -serviceName "social-coder-api-db" -projectFolder "$([IO.Path]::Combine($PSScriptRoot, "SocialCoder.Web", "Server"))" -image "$([System.Environment]::GetEnvironmentVariable("DB_DOCKER_IMAGE"))"