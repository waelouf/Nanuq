using FastEndpoints;
using Nanuq.Common.Audit;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Repositories;
using Nanuq.Common.Interfaces;
using Nanuq.Sqlite.Repositories;
using Serilog;
using Nanuq.Common.Repositories;
using Nanuq.EF;
using Microsoft.EntityFrameworkCore;
using Nanuq.Redis.Interfaces;
using Nanuq.Redis.Repository;
using Nanuq.RabbitMQ.Interfaces;
using Nanuq.RabbitMQ.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddFastEndpoints();

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.WriteTo.Console());

builder.Services.AddDbContext<NanuqContext>(
	opt =>
	{
		opt.UseSqlite(builder.Configuration.GetConnectionString("NanuqSqliteConfigurations"))
		.EnableSensitiveDataLogging()
		.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
	}
	);

// Database
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IKafkaRepository, KafkaRepository>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();
builder.Services.AddScoped<IRedisManagerRepository, RedisManagerRepository>();
builder.Services.AddScoped<IRabbitMqRepository, RabbitMqRepository>();
builder.Services.AddScoped<IRabbitMQManagerRepository, RabbitMQManagerRepository>();

builder.Services.AddScoped<ITopicsRepository, TopicsRepository>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var nanuqCorsPolicy = "_nanuqAllowOriginsHeadersMethods";

builder.Services.AddCors(options =>
{
	options.AddPolicy(nanuqCorsPolicy, policy =>
	{
		policy.AllowAnyOrigin()
		.AllowAnyHeader()
		.AllowAnyMethod();
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseFastEndpoints();
app.MapDefaultEndpoints();
app.UseCors();

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
