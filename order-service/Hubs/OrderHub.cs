using Microsoft.AspNetCore.SignalR;

namespace OrderService.Hubs;

/// <summary>
/// Пустой хаб — нужен только как end-point для SignalR.
/// </summary>
public sealed class OrderHub : Hub;