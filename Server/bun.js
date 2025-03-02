const readline = require("readline");

// Define our maps and game state.
const Maps = Object.freeze({
    LOBBY: 0
});

let currentMap = Maps.LOBBY;
let nextMap = Maps.LOBBY;

let nextWSId = 0;
const players = new Map();
let playerIdsPacket = Buffer.from([5, 0]);
const clients = new Map();

// --- UDP Server Setup ---
const udpSocket = await Bun.udpSocket({
    port: 1070,
    socket: {
        data(socket, buf, port, addr) {
            // For UDP, use a key like "udp:127.0.0.1:1070"
            const key = `udp:${addr}:${port}`;
            let client = clients.get(key);
            if (!client) {
                client = {
                    type: "udp",
                    key,
                    addr,
                    port,
                    pingTime: Date.now(),
                    ping: null,
                    playerId: null,
                    // For UDP, sending means calling the UDP socket send
                    send: (data) => {
                        socket.send(data, port, addr);
                    }
                };
                clients.set(key, client);
                log(`UDP client added: ${key}`);
            }
            client.pingTime = Date.now();
            // Process the incoming UDP message.
            const index = buf[0];
            const remainingData = buf.slice(1);
            if (Deserialize[index]) {
                Deserialize[index](client, remainingData);
            }
        }
    }
});
log(`UDP server running on port: ${udpSocket.port}`);

// --- WebSocket Server Setup ---
const wsSocket =Bun.serve({
    port: 1071,
    async fetch(req, server) {
        // Attempt to upgrade to WebSocket
        if (server.upgrade(req)) {
            return;
        }

        // Serve static files from the ./Build/ directory
        const url = new URL(req.url);
        let filePath = `./Build${url.pathname}`;

        // If requesting the root, serve index.html
        if (url.pathname === "/") {
            filePath = "./Build/index.html";
        }

        try {
            const file = Bun.file(filePath);
            if (!(await file.exists())) {
                return new Response("404 Not Found", { status: 404 });
            }
            return new Response(file);
        } catch (err) {
            return new Response("Internal Server Error", { status: 500 });
        }
    },
    websocket: {
        open(ws) {
            // Assign a unique key for the WebSocket client.
            const wsId = nextWSId++;
            const key = `ws:${wsId}`;
            const client = {
                type: "ws",
                key,
                ws,
                pingTime: Date.now(),
                ping: null,
                playerId: null,
                // For WS, sending means calling ws.send.
                send: (data) => {
                    ws.send(data);
                }
            };
            clients.set(key, client);
            log(`WebSocket client added: ${key}`);
        },
        message(ws, message) {
            // Find the client associated with this ws connection.
            let client;
            for (const [key, c] of clients.entries()) {
                if (c.type === "ws" && c.ws === ws) {
                    client = c;
                    break;
                }
            }
            if (!client) return;
            client.pingTime = Date.now();
            // Ensure we have a Buffer (if not, convert).
            let buf;
            if (Buffer.isBuffer(message)) {
                buf = message;
            } else if (message instanceof ArrayBuffer) {
                buf = Buffer.from(message);
            } else {
                buf = Buffer.from(message);
            }
            const index = buf[0];
            const remainingData = buf.slice(1);
            if (Deserialize[index]) {
                Deserialize[index](client, remainingData);
            }
        },
        close(ws, code, reason) {
            // On close, remove the client from our map.
            let clientKey;
            for (const [key, c] of clients.entries()) {
                if (c.type === "ws" && c.ws === ws) {
                    clientKey = key;
                    break;
                }
            }
            if (clientKey) {
                deleteClient(clientKey);
                log(`WebSocket client ${clientKey} disconnected`);
            }
        },
        error(ws, error) {
            log(`WebSocket error: ${error}`);
        }
    }
});

log(`WebSocket server running on port: ${wsSocket.port}`);


// --- Periodic Tasks ---
// Ping inactive clients and send a ping message (index 0).
setInterval(() => {
    const now = Date.now();
    clients.forEach((client, key) => {
        if (now - client.pingTime > 30000) {
            deleteClient(key);
            log(`Removed inactive client: ${key}`);
        } else {
            client.pingTime = now;
            client.send(Buffer.from([0])); // ping (index 0)
        }
    });
}, 3000);

// Send map and player data periodically.
setInterval(() => {
    let packet = Buffer.concat([Buffer.from([2, currentMap]), playerIdsPacket]);
    players.forEach((buffer, playerId) => {
        const prefix = Buffer.from([3]);
        const playerIdBuffer = Buffer.from([playerId]);
        const playerPacket = Buffer.concat([prefix, buffer, playerIdBuffer]);
        packet = Buffer.concat([packet, playerPacket]);
    });
    clients.forEach((client) => {
        const clientSpecificPacket = Buffer.concat([packet, Buffer.from([4, client.playerId])]);
        client.send(clientSpecificPacket);
    });
}, 20);

// --- Command Line Interface ---
const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

rl.on("line", (input) => {
    const args = input.trim().toLowerCase().split(/\s+/);
    switch (args[0]) {
        case "exit":
            log("Shutting down...");
            udpSocket.close();
            process.exit(0);
            break;
        case "map":
            if (args.length < 2) {
                console.log("Usage: map <mapName>");
                break;
            }
            const mapKey = args[1].toUpperCase();
            if (args.length > 2 && (args[2] === "now" || args[2] === "force")) {
                if (Maps[mapKey] !== undefined) {
                    currentMap = Maps[mapKey];
                    log(`Current map set to: ${currentMap}`);
                } else {
                    console.log(`Unknown map: ${args[1]}`);
                }
                break;
            }
            if (Maps[mapKey] !== undefined) {
                nextMap = Maps[mapKey];
                log(`Next map set to: ${nextMap}`);
            } else {
                console.log(`Unknown map: ${args[1]}`);
            }
            break;
        case "players":
            clients.forEach((value, key) => {
                console.log(`Key: ${key}, Player ID: ${value.playerId}`);
            });
            console.log(players.keys());
            break;
        default:
            log(`unrecognized command: ${input}`);
            break;
    }
});

// --- Message Deserialization ---
// The Deserialize array holds functions for each message index.
// We now use client.send instead of a UDP-specific send, and rely on client.key.
const Deserialize = [
    // 0 = pong
    (client, remainingData) => {
        const expectedData = Buffer.from([2, 0, 4, 4, 0, 1, 0, 2, 7]);
        if (remainingData.equals(expectedData)) {
            client.ping = Date.now() - client.pingTime;
        }
    },
    // 1 = connect
    (client, remainingData) => {
        const expectedData = Buffer.from([2, 0, 4, 4, 0, 1, 0, 2, 7, 1, 0, 7, 0]);
        if (remainingData.equals(expectedData)) {
            client.send(Buffer.from([1]));
            if (client.playerId === null) {
                const takenIds = new Set([...clients.values()].map(client => client.playerId));
                for (let i = 0; i < 256; i++) {
                    if (!takenIds.has(i)) {
                        client.playerId = i;
                        break;
                    }
                }
            }
        }
    },
    // 2 = disconnect
    (client, remainingData) => {
        const expectedData = Buffer.from([2, 0, 4, 4, 0, 1, 0, 2, 7, 3, 0, 7, 0]);
        if (remainingData.equals(expectedData)) {
            deleteClient(client.key);
            log(`Client ${client.key} disconnected`);
        }
    },
    // 3 = playerdata
    (client, remainingData) => {

        if (!players.has(client.playerId)) {
            players.set(client.playerId, remainingData);
            playersUpdated();
        } else {
            players.set(client.playerId, remainingData);
        }
    }
];

// --- Helper Functions ---
async function log(message) {
    const logEntry = `[${new Date().toISOString()}] ${message}`;
    console.log(logEntry);
}

function deleteClient(key) {
    const client = clients.get(key);
    if (client) {
        if (client.playerId !== null) {
            if (players.has(client.playerId)) {
                players.delete(client.playerId);
                playersUpdated();
            }
        }
        // If it's a WebSocket client, close its connection.
        if (client.type === "ws" && client.ws.readyState === WebSocket.OPEN) {
            client.ws.close();
        }
        clients.delete(key);
    }
}


function playersUpdated() {
    const playerIds = [];
    players.forEach((buffer, playerId) => {
        playerIds.push(playerId);
    });
    playerIdsPacket = Buffer.from([5, playerIds.length, ...playerIds]);
}
