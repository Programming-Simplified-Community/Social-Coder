FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

RUN apt install -y node
RUN npm install -y typescript -g

COPY ["SocialCoder.Web/Server/SocialCoder.Web.Server.csproj", "SocialCoder.Web/Server/"]
COPY ["SocialCoder.Web/Client/SocialCoder.Web.Client.csproj", "SocialCoder.Web/Client/"]
COPY ["SocialCoder.Web/Shared/SocialCoder.Web.Shared.csproj", "SocialCoder.Web/Shared/"]
RUN dotnet restore "SocialCoder.Web/Server/SocialCoder.Web.Server.csproj"
COPY . .
WORKDIR "/src/SocialCoder.Web/Server"
RUN dotnet build "SocialCoder.Web.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SocialCoder.Web.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialCoder.Web.Server.dll"]
