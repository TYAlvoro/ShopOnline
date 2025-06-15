using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o =>
    o.AddPolicy("frontend", p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        routes: new[]
        {
            new RouteConfig
            {
                RouteId  = "orders",
                ClusterId = "orders-cluster",
                Match    = new() { Path = "/orders/{**catchAll}" }
            },
            new RouteConfig
            {
                RouteId  = "orders-hub",
                ClusterId = "orders-cluster",
                Match    = new() { Path = "/hub/orders/{**catchAll}" }
            },
            new RouteConfig
            {
                RouteId  = "payments",
                ClusterId = "payments-cluster",
                Match    = new() { Path = "/payments/{**catchAll}" }
            }
        },
        clusters: new[]
        {
            new ClusterConfig
            {
                ClusterId    = "orders-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["primary"] = new() { Address = "http://order-service:8080/" }
                }
            },
            new ClusterConfig
            {
                ClusterId    = "payments-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["primary"] = new() { Address = "http://payment-service:8080/" }
                }
            }
        });

var app = builder.Build();

app.UseWebSockets();   // критично для проксирования SignalR
app.UseCors("frontend");
app.MapReverseProxy();

app.Run();
