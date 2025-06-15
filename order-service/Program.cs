using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Hubs;
using OrderService.Services;
using ShopOnline.Shared.Messaging;
using ShopOnline.Shared.Outbox;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("OrdersDb")));

builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();

builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<OutboxPublisher<OrdersDbContext>>();

builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHub<OrderHub>("/orders");

app.Run();