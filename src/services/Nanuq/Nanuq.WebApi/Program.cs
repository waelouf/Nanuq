using FastEndpoints;
using Nanuq.Common.Audit;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Repositories;
using Nanuq.Sqlite;
using Nanuq.Sqlite.Interfaces;
using Nanuq.Sqlite.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddFastEndpoints();

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.WriteTo.Console());

builder.Services.AddSingleton<IDbContext, DbContext>();
builder.Services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddSingleton<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddSingleton<IKafkaRepository, KafkaRepository>();
builder.Services.AddSingleton<ITopicsRepository, TopicsRepository>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseFastEndpoints();

app.MapDefaultEndpoints();

try
{
	Log.Information("App started");
	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Host terminated");
}
finally
{
	await Log.CloseAndFlushAsync();
}
