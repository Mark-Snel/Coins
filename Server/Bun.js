const readline = require("readline");

const clients = new Map();
const socket = await Bun.udpSocket({
    port: 1070,
    socket: {
        data(socket, buf, port, addr) {
            const currentTime = Date.now();
            if (!clients.has(addr + ":" + port)) {
                clients.set(addr + ":" + port, { addr, port, lastActive: currentTime, ping: null });
                log(`Client added: ${addr + ":" + port}`);
            }
            const key = addr + ":" + port;
            const client = clients.get(key);
            client.lastActive = currentTime;
            const message = buf.toString();
            switch (message) {
                case "pong":
                    client.ping = Date.now() - client.lastActive;
                    break;
                case "hello server, it is i, coins client":
                    socket.send("greetings coins client, a wonderous meeting indeed", port, addr);
                    break;
                case "disconnecting":
                    clients.delete(key);
                    log(`Client ${key} disconnected.`);
                    break;
            }
        }
    }
});
log(`UDP server running on port: ${socket.port}`);

setInterval(() => {
    const now = Date.now();
    clients.forEach((client, key) => {
        const { addr, port, lastActive } = client;
        if (now - lastActive > 30000) {
            clients.delete(key);
            log(`Removed inactive client: ${key}`);
        } else {
            socket.send("ping", port, addr);
        }
    });
}, 3000);

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

rl.on("line", (input) => {
    if (input.trim().toLowerCase() === "exit") {
        log("Shutting down...");
        socket.close();
        process.exit(0);
    } else {
        log(`Command: ${input}`);
    }
});

async function log(message) {
    const logEntry = `[${new Date().toISOString()}] ${message}`;
    console.log(logEntry);
}
