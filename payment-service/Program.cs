using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Messaging;
using PaymentService.Services;
using ShopOnline.Shared.Messaging;
using ShopOnline.Shared.Outbox;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("PaymentsDb")));

builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<OutboxPublisher<PaymentsDbContext>>();
builder.Services.AddHostedService<KafkaConsumer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var application = builder.Build();
application.UseSwagger();
application.UseSwaggerUI();
application.MapControllers();
application.Run();