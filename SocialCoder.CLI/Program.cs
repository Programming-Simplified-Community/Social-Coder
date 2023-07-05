using SocialCoder.CLI.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<InitializeSettings>("init")
        .WithDescription("Initialize settings for your development environment");

    config.AddCommand<UpdateSettings>("settings")
        .WithDescription("Update a specific section of your development settings");

    config.AddCommand<StartDatabaseCommand>("start-db")
        .WithDescription("Starts Social-Coder's database service. Then automatically applies database migrations");

    config.AddCommand<ResetDatabaseCommand>("reset-db")
        .WithDescription("Tears down Social-Coder's database service. Deletes the persistent directory, then starts the service back up");

    config.AddCommand<CertCommand>("gen-ssl")
        .WithDescription("Generates SSL certificate chain for Social Coder. Uses OpenSSL to do so");
});

app.Run(args);