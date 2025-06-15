using Microsoft.EntityFrameworkCore;
using OrderService.Data;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var application = builder.Build();
application.UseSwagger();
application.UseSwaggerUI();
application.MapControllers();
application.Run();