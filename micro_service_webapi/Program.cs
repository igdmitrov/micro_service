using micro_service_shared;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

var settings = builder.Configuration
                    .GetSection("AppRabbitMqConnection")
                    .Get<AppRabbitMqSettings>();

// Add services to the container.

builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>(serviceProvider =>
{
    return new ConnectionFactory
    {
        HostName = settings.Host,
        Port = settings.Port,
        UserName = settings.UserName,
        Password = settings.Password,
        DispatchConsumersAsync = true
    };
});

builder.Services.AddSingleton<IBusClient, RabbitMqClient>();

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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

