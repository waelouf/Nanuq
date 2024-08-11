var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Nanuq_WebApi>("api");

builder.Build().Run();
