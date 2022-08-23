
# TODO: Make this into something reusable for either deployment / development. Perhaps parameterize password
# - if password is parameterized we need to ensure our docker-compose file is also updated? right? I think?
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p password
dotnet dev-certs https --trust