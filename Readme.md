# Status
[![CodeQL](https://github.com/Programming-Simplified-Community/Social-Coder/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/Programming-Simplified-Community/Social-Coder/actions/workflows/codeql-analysis.yml)
[![MegaLinter](https://github.com/programming-simplified-community/social-coder/workflows/MegaLinter/badge.svg?branch=main)](https://github.com/programming-simplified-community/social-coder/actions?query=workflow%3AMegaLinter+branch%3Amain)

# Social Coder

This project is geared towards building a social-media-like experience for programmers!

Clearly, this is a work in progress app but here's a sneak peak on current progress!
![WIP Code Jam Topics](imgs/code-jam-topics.png)
1. OAuth login via Google, Discord, GitHub (will add more)
2. Ability to view code jams
3. Ability to register/withdraw from code jams


## Contributing

If you're interested in contributing here's [Environment](Environment.md) setup instructions/suggestions.

We are using dotnet Aspire to help manage dependent services such as creating a Postgres database via Docker. 

### Running the project

Assuming your environment is set up, run the project by running the AppHost project. This is our Aspire orchestrator that automatically manages the database
portion of this project.

If running via Rider, you can run the project by pressing the green play button - with `SocialCoder.AppHost.https` selected.

Alternatively, you can run the project via the command line:

```Bash
cd SocialCoder.AppHost
dotnet watch run
```

## Services

After some research we have determined that we needed something a bit better than standard REST endpoints between our **client** and **server**. 
Despite being noobs, we're tackling GraphQL as it looks like an excellent long-term solution for loading an entire pages worth of data in one go... compared to
calling `N` amount of endpoints for `N` components on screen.

## Login
There are no default credentials. Must use OAuth. The setup page has links to each OAuth provider's documentation to help you get started.

## OAuth Setup

During the first launch of the application, you will see a "setup-mode." While in this mode, the application is not 
usable. You're required to ensure that you have at least one OAuth provider, and a valid database connection. If using our Aspire AppHost, you should
already have a database connection string provided for you. If not, you will need to create a database and provide the connection string.

Once both requirements are satisfied, you can finalize the setup by clicking the "Finish Setup" button. Then the server
will shut down. If in a containerized environment, if the restart policy is set to 'always' or 'unless-stopped' the server will automatically restart. Otherwise,
you will need to manually start the server. Additionally, if you're using the Aspire AppHost, you can press the "Start" button on the main dashboard to start the server.

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