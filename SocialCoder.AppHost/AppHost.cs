var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.SocialCoder_Web_Server>("socialcoder-server");

var postgres = builder.AddPostgres("socialcoder-db")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImagePullPolicy(ImagePullPolicy.Always)
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin()
    .AddDatabase("socialcoder");

server.WithReference(postgres)
    .WaitFor(postgres);


builder.Build().Run();
