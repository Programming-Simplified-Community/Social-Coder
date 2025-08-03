# IDE

This is ultimately developer preference. These are the suggested IDEs

**Free**

[Visual Studio Code](https://code.visualstudio.com/download)

[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

**Free for open source**

[JetBrains Rider](https://www.jetbrains.com/rider/download/#section=windows)

-----

# Dev Container

Rather than installing a bunch of stuff locally, such as the right dotnet SDK you can use a dev container!

You will, however, need to have docker installed locally.

## Rider

Click the blue cube,

![img.png](./imgs/dev-container-1.png)

Then press **Create Dev Container and Mount Sources -> Rider**

![img.png](./imgs/dev-container-2.png)

Rider will then create the dev-container, mounting Rider's backend IDE. When
 complete, you will be able to connect to the dev-container.

![img.png](./imgs/dev-container-3.png)

-----

# Framework
We're utilizing C# Net 9.0 right now
[Net 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

We're utilizing Discord.NET as our bot library

[Discord.Net Doc Site](https://discordnet.dev/)

[Discord.NET Github](https://github.com/discord-net/Discord.Net)

# Docker
If you are testing, or adding to the Challenge Assistant service Docker **IS REQUIRED**.
[Docker](https://docs.docker.com/get-docker/)

Right now, **MySql** is used for our database. Which docker takes care of. If you aren't using docker, you will need to grab MySQL from their website:
[Download](https://dev.mysql.com/downloads/mysql/)

Your connection string can be updated using the steps [here](Settings.md), except the keyname would be `ConnectionStrings:Default`. You can reference `appsettings.json` to see how to format the connection string. Please reference the [database](database.md) doc on more details for db setup.

# Misc
Make sure you have [Git](https://git-scm.com/downloads) installed on your machine!

