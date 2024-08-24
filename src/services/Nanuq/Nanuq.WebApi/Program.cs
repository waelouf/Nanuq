using FastEndpoints;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Repositories;
using Nanuq.Sqlite;
using Nanuq.Sqlite.Interfaces;
using Nanuq.Sqlite.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddFastEndpoints();

builder.Services.AddSingleton<IDbContext, DbContext>();
builder.Services.AddSingleton<IKafkaRepository, KafkaRepository>();
builder.Services.AddSingleton<ITopicsRepository, TopicsRepository>();
builder.Services.AddSingleton<IActivityLogRepository, ActivityLogRepository>();

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

app.Run();
