using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithImageRegistry("ghcr.io")
    .WithImage("pgmq/pg18-pgmq", "v1.10.0")
    .WithVolume("summerborninfo_postgresdata", "/var/lib/postgresql/18/docker")
    .WithPgAdmin()
    .WithContainerRuntimeArgs("--user", "root");

var postgresdb = postgres.AddDatabase("SummerbornInfo");

builder.AddProject<SummerBornInfo_Web>("API")
    .WaitFor(postgresdb)
    .WithReference(postgresdb);

builder.Build().Run();
