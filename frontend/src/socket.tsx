import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

export let connection: HubConnection | null = null;

export function connect(): HubConnection {
    if (connection) return connection;

    connection = new HubConnectionBuilder()
        // API-gateway бегает на 80-м порту, маршрут /orders проксируется в Order-service
        .withUrl("http://localhost/orders")
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

    return connection;
}
