using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Hubs;
using OrderService.Services;
using ShopOnline.Shared.Messaging;
using ShopOnline.Shared.Outbox;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddDbContext<OrdersDbContext>(o =>
    o.UseNpgsql(cfg.GetConnectionString("OrdersDb")));

builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<OutboxPublisher<OrdersDbContext>>();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<HostOptions>(h =>
    h.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHub<OrderHub>("/orders");
app.Run();