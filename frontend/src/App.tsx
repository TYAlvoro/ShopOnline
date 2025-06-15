import { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const HUB_URL = "http://localhost/hub/orders";

export default function App() {
    const [status, setStatus] = useState<string | null>(null);
    const [connected, setConnected] = useState(false);

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl(HUB_URL)
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        connection
            .start()
            .then(() => setConnected(true))
            .catch(() => setStatus("❌ Не удалось подключиться к хабу"));

        connection.on("PaymentCompleted", () =>
            setStatus("🟢 Заказ оплачен"));
        connection.on("PaymentFailed", () =>
            setStatus("🔴 Недостаточно средств"));

        return () => void connection.stop();
    }, []);

    return (
        <main style={{ fontFamily: "sans-serif", padding: "2rem" }}>
            <h1>ShopOnline demo front-end</h1>

            <p>{connected ? "Соединение с SignalR установлено" : "Подключаемся к SignalR…"}</p>

            {status && <p style={{ marginTop: "1rem", fontSize: "1.25rem" }}>{status}</p>}

            <p style={{ marginTop: "2rem" }}>
                Создайте заказ через Swagger и наблюдайте уведомления здесь.
            </p>
        </main>
    );
}
