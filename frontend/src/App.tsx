import { useEffect, useState } from "react";
import {
    HubConnection,
    HubConnectionBuilder,
    LogLevel
} from "@microsoft/signalr";

const HUB_URL = "http://localhost/orders"; // прокси-маршрут из YARP

export default function App() {
    // ---------------- state ----------------
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [status, setStatus] = useState<string | null>(null);
    const [connected, setConnected] = useState(false);

    // ---------------- effects --------------
    useEffect(() => {
        // создаём и запоминаем соединение
        const conn = new HubConnectionBuilder()
            .withUrl(HUB_URL)          // обратите внимание: URL без ws://
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        setConnection(conn);

        // стартуем
        conn
            .start()
            .then(() => setConnected(true))
            .catch(err => {
                console.error("SignalR connection error:", err);
                setStatus("❌ Не удалось подключиться к хабу");
            });

        // события из backend
        conn.on("PaymentCompleted", () =>
            setStatus("🟢 Заказ оплачен")
        );
        conn.on("PaymentFailed", () =>
            setStatus("🔴 Недостаточно средств")
        );

        // отписка при размонтировании
        return () => {
            conn.stop().catch(console.error);
        };
    }, []);

    // --------------- ui --------------------
    return (
        <main style={{ fontFamily: "sans-serif", padding: "2rem" }}>
            <h1>ShopOnline demo front-end</h1>

            <p>
                {connected
                    ? "Соединение с SignalR установлено"
                    : "Подключаемся к SignalR…"}
            </p>

            {status && (
                <p style={{ marginTop: "1rem", fontSize: "1.25rem" }}>{status}</p>
            )}

            <p style={{ marginTop: "2rem" }}>
                Создайте заказ через Swagger и наблюдайте уведомления здесь.
            </p>
        </main>
    );
}
