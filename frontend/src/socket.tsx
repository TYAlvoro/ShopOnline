import { io } from "socket.io-client";

export const socket = io("ws://localhost/orders", {
    transports: ["websocket"],
    autoConnect: false
});
