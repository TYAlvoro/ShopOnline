import { useEffect, useState } from "react";
import { socket } from "./socket";

export default function App() {
    const [statusMessage, setStatusMessage] = useState<string | null>(null);

    useEffect(() => {
        socket.connect();

        socket.on("PaymentCompleted", () =>
            setStatusMessage("🟢 Заказ оплачен")
        );
        socket.on("PaymentFailed", () =>
            setStatusMessage("🔴 Недостаточно средств")
        );

        return () => socket.disconnect();
    }, []);

    return (
        <main style={{ fontFamily: "sans-serif", padding: "2rem" }}>
            <h1>ShopOnline demo front-end</h1>
            {statusMessage && (
                <p style={{ marginTop: "1rem", fontSize: "1.25rem" }}>
                    {statusMessage}
                </p>
            )}
            <p>
                Создайте заказ через Swagger и наблюдайте уведомления здесь.
            </p>
        </main>
    );
}
