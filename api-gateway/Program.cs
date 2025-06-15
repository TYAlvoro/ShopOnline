using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        routes:
        [
            new RouteConfig
            {
                RouteId = "orders",
                ClusterId = "orders-cluster",
                Match = new RouteMatch { Path = "/orders/{**catchAll}" }
            },
            new RouteConfig
            {
                RouteId = "payments",
                ClusterId = "payments-cluster",
                Match = new RouteMatch { Path = "/payments/{**catchAll}" }
            }
        ],
        clusters:
        [
            new ClusterConfig
            {
                ClusterId = "orders-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["primary"] = new DestinationConfig
                    {
                        Address = "http://order-service:8080/"
                    }
                }
            },
            new ClusterConfig
            {
                ClusterId = "payments-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["primary"] = new DestinationConfig
                    {
                        Address = "http://payment-service:8081/"
                    }
                }
            }
        ]);

var application = builder.Build();
application.MapReverseProxy();
application.Run();