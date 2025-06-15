using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Messaging;
using PaymentService.Services;
using ShopOnline.Shared.Messaging;
using ShopOnline.Shared.Outbox;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddDbContext<PaymentsDbContext>(o =>
    o.UseNpgsql(cfg.GetConnectionString("PaymentsDb")));

builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<OutboxPublisher<PaymentsDbContext, Hub>>();
builder.Services.AddHostedService<KafkaConsumer>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<HostOptions>(h =>
    h.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();