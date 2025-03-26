using System;
using System.Threading.Tasks;

public interface IConnection {
    Task Connect(string address);
    Task Disconnect();
    Task Send(byte[] data);
    event Action<byte[]> OnDataReceived;
    event Action<ConnectionStatus> OnStatusChanged;
}

public enum ConnectionStatus {
    Disconnected,
    Connecting,
    Connected
}
