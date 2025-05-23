using Play.Common.Identity;
using Play.Common.MongoDB;
using Play.Common.MassTransit;
using Play.Inventory.API.Clients;
using Play.Inventory.API.Entities;
using Polly;
using Polly.Timeout;

const string AllowedOriginSetting = "AllowedOrigin";

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
}).CreateLogger("Program");

var jitterer = new Random();

// Add services to the container.
builder.AddMongo()
       .AddMongoRepository<InventoryItem>("inventoryitems")
       .AddMongoRepository<CatalogItem>("catalogitems")
       .AddMassTransitWithRabbitMQ()
       .AddJwtBearerAuthentication();

AddCatalogClient(builder, logger, jitterer);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(builder => 
    {
        builder.WithOrigins(configuration[AllowedOriginSetting]!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogClient(WebApplicationBuilder builder, ILogger logger, Random jitterer)
{
    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5234");
    })
    .AddTransientHttpErrorPolicy(policy => policy.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
        onRetry: (outcome, timespan, retryAttempt) =>
        {
            logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
        }))
    .AddTransientHttpErrorPolicy(policy => policy.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            logger.LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
        },
        onReset: () =>
        {
            logger.LogWarning($"Closing the circuit...");
        }
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}