var builder = DistributedApplication.CreateBuilder(args);

var apiapp = builder.AddProject<Projects.eShopLite_ApiApp>("apiapp");

builder.AddProject<Projects.eShopLite_WebApp>("webapp")
       .WithExternalHttpEndpoints()
       .WithReference(apiapp);

builder.Build().Run();
