# Status
[![CodeQL](https://github.com/Programming-Simplified-Community/Social-Coder/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/Programming-Simplified-Community/Social-Coder/actions/workflows/codeql-analysis.yml)
[![MegaLinter](https://github.com/programming-simplified-community/social-coder/workflows/MegaLinter/badge.svg?branch=main)](https://github.com/programming-simplified-community/social-coder/actions?query=workflow%3AMegaLinter+branch%3Amain)

# Social Coder

This project is geared towards building a social-media like experience for programmers!

Clearly, this is a work in progress app but here's a sneak peak on current progress!
![WIP Code Jam Topics](imgs/code-jam-topics.png)
1. OAuth login via Google or Discord (will add more)
2. Ability to view code jams
3. Ability to register/withdraw from code jams

If you're interested in contributing here's [Environment](Environment.md) setup instructions/suggestions.

Of course, no app is complete without a [Database](Database.md)!

## Scripts

Before starting the database via docker-compose we need to setup some environment variables.

There are a few parameters you can specify if you want... running it with no parameters will
use default values for our connection string! Which is fine...

[View Script](create-env.ps1)

Please note this will override previously set values if you don't respecify them!
Additionally, this script will automatically update your connection string.

```bash
./create-env.ps1

./create-env.ps1 -databasePath Path\To\Where\You\Want\To\Store\Db
```

Start database from [docker-compose](docker-compose.yml)
```bash
./start-db.ps1
```

Create https-certs. Depending on your environment your IDE might have already set this up for you.
Otherwise, this script will do the trick... hopefully

```bash
./create-https-certs.ps1
```

## Services

After some research we have determined that we needed something a bit better than standard REST endpoints between our **client** and **server**. 
Though we're noobs to implement GraphQL, it looks like an excellent long-term solution for loading an entire pages worth of data in one go... compared to
calling `N` amount of endpoints for `N` components on screen.

```mermaid
graph TD
    Client[Client] --- GraphQL[GraphQL]
    GraphQL --- Server
    Server --- Postgres[Postgres DB]
```

## Login
There are no default credentials. Must utilize OAuth.

## OAuth Setup

Will require adding a `appsettings.development.json` to your Server project [SocialCoder.Web](SocialCoder.Web)

```json
{
  "Authentication": {
    "ProviderName": {
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

Right now we have Discord and Google setup for our application. A requirement to set this up will be to register with 
the following services:

#### Discord

[Discord Developer Portal](https://discord.com/developers/applications)

- Create an application (if you don't have one already)
- Go to OAuth2 panel
  - Grab both the `Client ID` and `Client Secret` and put them into the appropriate place in your `appsettings.development.json`. The ProviderName for this will be `Discord`... because....it's... discord.
  - Update the redirect to be `https://localhost:7159/signin-discord`

#### Google

[Google Portal](https://console.cloud.google.com)

- Create a project to use for testing
- Go to API & Services --> Credentials
  - Create Credentials 
    - Add Authorized `JavaScript origins`: `https://localhost:7159`
    - Authorized Redirect URIs: `https://localhost:7159/signin-google`
    - Save and grab the `Client ID` and `Client Secret`. Shove this into `Google`. Should be self explanatory that the pattern here is `ProviderName` equals the Service name we're using for OAuth.

#### Github

[Register Github App](https://github.com/settings/applications/new)

- Application name can be whatever you want for testing purposes.
- Homepage URL
  - https://github.com/Programming-Simplified-Community/Social-Coder
- Authorization callback URL
  - https://localhost:7159/signin-github
- Grab the ClientID, and ClientSecret and put them in the `appsettings.development.json` file under `Github`