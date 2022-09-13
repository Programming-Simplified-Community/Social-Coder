$scriptPath = Split-Path $MyInvocation.MyCommand.Path -Parent
cd $scriptPath

cd "$([IO.Path]::Combine($scriptPath, "SocialCoder.Web", "Server"))"

dotnet watch