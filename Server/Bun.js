const socket = await Bun.udpSocket({ port: 1070 });

console.log(`UDP server running on port: ${socket.port}`);

socket.on("message", (msg, info) => {
    console.log(`Received message: ${msg.toString()} from ${info.address}:${info.port}`);

    const replyMessage = "Message received!";
    socket.send(replyMessage, info.port, info.address);
});
