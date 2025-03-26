const readline = require("readline");

// Define our maps and game state.
const Maps = Object.freeze({
    LOBBY: 0
});

let currentMap = Maps.LOBBY;
let nextMap = Maps.LOBBY;

const deathTracker = new DeathTracker();

const players = new Map();
const shots = new Map();
const hits = new Map();
let playerIdsPacket = Buffer.from([5, 0]);
const clients = new Map();

// --- WebSocket Server Setup ---
const wsSocket =Bun.serve({
    port: 1071,
    async fetch(req, server) {
        // Attempt to upgrade to WebSocket
        if (server.upgrade(req, {
            data: {
                id: req.headers.get("sec-websocket-key")
            }
        })) {
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
            const wsId = ws.data.id;
            const key = `ws:${wsId}`;

            const client = {
                type: "ws",
                key,
                ws,
                pingTime: Date.now(),
                ping: null,
                playerId: null,
                send: (data) => {
                    ws.send(data);
                },
                died: false
            };
            clients.set(key, client);
            log(`WebSocket client added: ${key}`);
            const takenIds = new Set([...clients.values()].map(client => client.playerId));
            for (let i = 0; i < 256; i++) {
                if (!takenIds.has(i)) {
                    client.playerId = i;
                    break;
                }
            }
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
    let roundComplete = false;
    if (players.size === 1) {
        if (deathTracker.getDeathOrder().length > 0) {
            roundComplete = true;
        }
    } else if (players.size > 1) {
        if (deathTracker.GetDeathOrder().length >= players.size - 1) {
            roundComplete = true;
        }
    }

    let packet = Buffer.concat([Buffer.from([2, currentMap]), playerIdsPacket]);
    players.forEach((buffer, playerId) => {
        const prefix = Buffer.from([3]);
        const playerIdBuffer = Buffer.from([playerId]);
        const playerPacket = Buffer.concat([prefix, buffer, playerIdBuffer]);
        packet = Buffer.concat([packet, playerPacket]);
    });
    clients.forEach((client) => {
        if (client.playerId !== null) {
            let shotDataArray = [];
            shots.forEach((shotBuffer, shotPlayerId) => {
                if (shotPlayerId !== client.playerId) {
                    const playerIdBuffer = Buffer.from([shotPlayerId]);
                    const shotBufferWithId = Buffer.concat([playerIdBuffer, shotBuffer]);
                    shotDataArray.push(shotBufferWithId);
                }
            });
            let shotsPacket = Buffer.alloc(0);
            if (shotDataArray.length > 0) {
                const identifierBuffer = Buffer.alloc(2);
                identifierBuffer.writeUInt8(6, 0);
                identifierBuffer.writeUInt8(shotDataArray.length, 1);
                shotsPacket = Buffer.concat([identifierBuffer, ...shotDataArray]);
            }

            let hitDataArray = [];
            hits.forEach((hitBuffer, hitPlayerId) => {
                if (hitPlayerId !== client.playerId) {
                    hitDataArray.push(hitBuffer);
                }
            });
            let hitsPacket = Buffer.alloc(0);
            if (hitDataArray.length > 0) {
                const identifierBuffer = Buffer.alloc(2);
                identifierBuffer.writeUInt8(7, 0);
                identifierBuffer.writeUInt8(hitDataArray.length, 1);
                hitsPacket = Buffer.concat([identifierBuffer, ...hitDataArray]);
            }

            if (roundComplete) {
                let roundOverPacket = Buffer.from([8, Math.min(deathTracker.getDeathOrder(client.playerId) + 1, 3), deathTracker.getWinner()]);
            }

            const clientSpecificPacket = Buffer.concat([packet, shotsPacket, hitsPacket, Buffer.from([4, client.playerId])]);
            client.send(clientSpecificPacket);
        }
    });
    if (roundComplete) {
        deathTracker.reset();
        Log("Round Completed");
    }
    shots.clear();
    hits.clear();
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
        case "shots":
            console.log(`Total players with shots: ${shots.size}`);
            for (const [playerId, shotPacket] of shots.entries()) {
                const count = shotPacket.readInt32LE(0);
                console.log(`Player ${playerId} has ${count} shots.`);
            }
            break;
        case "hits":
            console.log(`Total players with hits: ${hits.size}`);
            for (const [playerId, hitPacket] of hits.entries()) {
                const count = hitPacket.readInt32LE(0);
                console.log(`Player ${playerId} has ${count} hits.`);
                console.log(hitPacket);
            }
            break;
        default:
            log(`unrecognized command: ${input}`);
            break;
    }
});

// --- Message Deserialization ---
// The Deserialize array holds functions for each message index.
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

    },
    // 2 = disconnect
    (client, remainingData) => {

    },
    // 3 = playerdata
    (client, remainingData) => {
        if (client.playerId !== null) {
            if (remainingData.length < PLAYER_DATA_SIZE) {
                return;
            }
            const playerData = remainingData.slice(0, PLAYER_DATA_SIZE);
            const nextData = remainingData.slice(PLAYER_DATA_SIZE);

            if (playerData[0] === 1 && !client.died) {
                deathTracker.addDeath(client.playerId);
                client.died = true;
            }

            if (!players.has(client.playerId)) {
                players.set(client.playerId, playerData);
                playersUpdated();
            } else {
                players.set(client.playerId, playerData);
            }

            // If there's more data left, continue deserializing
            continueDeserializing(client, nextData);
        }
    },
    // 4 = shots
    (client, remainingData) => {
        if (client.playerId !== null) {
            const count = remainingData.readInt32LE(0);
            const totalLength = 4 + 40 * count;

            if (remainingData.length !== totalLength) {
                return;
            }

            const shotPacket = remainingData.slice(0, totalLength);
            if (shots.has(client.playerId)) {
                const prevBuffer = shots.get(client.playerId);
                const prevCount = prevBuffer.readInt32LE(0);
                const prevShots = prevBuffer.slice(4);

                const newShots = shotPacket.slice(4);

                const totalCount = prevCount + count;
                const combinedBuffer = Buffer.alloc(4 + prevShots.length + newShots.length);
                combinedBuffer.writeInt32LE(totalCount, 0);
                prevShots.copy(combinedBuffer, 4);
                newShots.copy(combinedBuffer, 4 + prevShots.length);

                shots.set(client.playerId, combinedBuffer);
            } else {
                shots.set(client.playerId, shotPacket);
            }


            const nextData = remainingData.slice(totalLength);
            continueDeserializing(client, nextData);
        }
    },
    // 5 = hits
    (client, remainingData) => {
        if (client.playerId !== null) {
            const count = remainingData.readInt32LE(0);
            const totalLength = 4 + 18 * count;
            if (remainingData.length !== totalLength) {
                return;
            }

            const hitPacket = remainingData.slice(0, totalLength);
            if (hits.has(client.playerId)) {
                const prevBuffer = hits.get(client.playerId);
                const prevCount = prevBuffer.readInt32LE(0);
                const prevHits = prevBuffer.slice(4);

                const newHits = hitPacket.slice(4);

                const totalCount = prevCount + count;
                const combinedBuffer = Buffer.alloc(4 + prevHits.length + newHits.length);
                combinedBuffer.writeInt32LE(totalCount, 0);
                prevHits.copy(combinedBuffer, 4);
                newHits.copy(combinedBuffer, 4 + prevHits.length);

                hits.set(client.playerId, combinedBuffer);
            } else {
                hits.set(client.playerId, hitPacket);
            }


            const nextData = remainingData.slice(totalLength);
            continueDeserializing(client, nextData);
        }
    }
];

function continueDeserializing(client, nextData) {
    if (nextData.length > 0) {
        const nextIndex = nextData[0];
        const nextRemainingData = nextData.slice(1);
        if (Deserialize[nextIndex]) {
            Deserialize[nextIndex](client, nextRemainingData);
        }
    }
}

const PLAYER_DATA_SIZE = 85;

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









// --- Gaming ---
class DeathTracker {
    constructor() {
        this.index = 0;
        this.deaths = new Map();
        this.winner = null;
        this.lastAddedPlayer = null;
    }
    addDeath(playerId) {
        if (!this.deaths.has(playerId)) {
            this.deaths.set(playerId, this.index++);
        }
        this.lastAddedPlayer = playerId;
    }
    getDeathOrder(playerId) {
        return this.deaths.get(playerId);
    }
    reset() {
        this.winner = null;
        this.index = 0;
        this.deaths.clear();
        this.lastAddedPlayer = null;
    }
    getWinner() {
        if (this.winner == null) {
            let alivePlayer = null;
            for (const playerId in this.players) {
                if (!this.deaths.has(playerId)) {
                    alivePlayer = playerId;
                    break;
                }
            }

            if (!alivePlayer && Object.keys(this.players).length === 1) {
                alivePlayer = Object.keys(this.players)[0];
            } else if (!alivePlayer) {
                alivePlayer = this.lastAddedPlayer;
            }
            this.winner = alivePlayer;
        }

        return this.winner;
    }
}
