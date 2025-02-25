using System;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Serializer;
using Wildlife.PitayaCSharp.Event;

namespace Wildlife.PitayaCSharp.Transport
{
    public interface ITransporter
    {
        IPitayaClient Client { get; }
        IPitayaClientConfig ClientConfig { get; }
        int Quality { get; }
        ProtobufSerializer.SerializationFormat? Serializer { get; }
        ITransporterPlugin Plugin { get; }
        TransportState State { get; }

        void Connect(string host, int port, string handshakeOpts);
        void Send(MessageType messageType, string route, uint sequenceNumber, byte[] data, uint requestUid, int timeout);
        void Send(byte[] packetBuffer, uint sequenceNumber, uint requestUid, int timeout); // This shouldn't be exposed here, but in a deeper layer.
        void Disconnect();

        // Consider the need to separate concerns
        void AddEventHandler(Action<IClientEvent> onClientEvent);
        void SetPushHandler(Action<string, byte[]> onPush);
    }
}
