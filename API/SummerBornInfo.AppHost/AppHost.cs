using Projects;
using SummerbornInfo.PostgresDockerImage;

var builder = DistributedApplication.CreateBuilder(args);
var appHostDirectory = PostgresDockerfilePath.PostgreSqlDockerfileDirectory;

var postgres = builder
    .AddPostgres("postgres")
    .WithDockerfile(Path.Combine(appHostDirectory, "Postgres"))
    .WithVolume("summerborninfo_postgresdata", "/var/lib/postgresql/18/docker")
    .WithPgAdmin()
    .WithContainerRuntimeArgs("--user", "root");

var postgresdb = postgres.AddDatabase("SummerbornInfo");

builder.AddProject<SummerBornInfo_Web>("API")
    .WaitFor(postgresdb)
    .WithReference(postgresdb);

await builder.Build().RunAsync();
