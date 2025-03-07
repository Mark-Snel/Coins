using System;

public interface IConnection {
    void Connect(string address);
    void Connected();
    void Disconnect();
    void Send(byte[] data);
    event Action<byte[]> OnDataReceived;
}

public enum ConnectionStatus {
    Disconnected,
    Connecting,
    Connected
}
