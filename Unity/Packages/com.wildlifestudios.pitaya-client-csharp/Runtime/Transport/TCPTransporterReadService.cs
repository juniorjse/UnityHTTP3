using System;
using System.IO;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Event;

namespace Wildlife.PitayaCSharp.Transport
{
    class StateObject
    {
        const int _readBufferSize = 1024;
        internal byte[] _readBuffer = new byte[_readBufferSize];
    }

    public class TCPTransporterReadService
    {
        Stream _stream;
        PacketParser _packetParser;
        bool _isRunning;
        internal Action _onReconnect = null;
        ITransporter _transporter;
        TransporterNotifyService _notifier;

        public TCPTransporterReadService(Action<Packet> onPacketHandler, ITransporter transporter, TransporterNotifyService notifier)
        {
            _packetParser = new PacketParser(onPacketHandler);
            _isRunning = false;
            _transporter = transporter;
            _notifier = notifier;
        }

        public void Start(Stream readStream)
        {
            _stream = readStream;
            _isRunning = true;

            var state = new StateObject();
            Read(state);
        }

        void Read(StateObject state)
        {
            if (_isRunning)
            {
                try
                {
                    _stream.BeginRead(state._readBuffer, 0, state._readBuffer.Length,
                        new AsyncCallback(OnRead), state);
                }
                catch
                {
                    PitayaClientLib.Logger.LogError("tcp__conn_done_cb - start read from tcp error, reconn");
                    _onReconnect?.Invoke();
                }
            }
        }

        internal void Close()
        {
            _isRunning = false;

            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            }
        }

        private void OnRead(IAsyncResult asyncReceive)
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                var state = (StateObject)asyncReceive.AsyncState;

                int length = _stream.EndRead(asyncReceive);

                if (length < 0)
                {
                    // TODO: uv_strerror(length)
                    PitayaClientLib.Logger.LogError(string.Format("tcp__on_tcp_read_cb - read from tcp error: , {0}", "will reconn"));

                    if (_transporter.State == TransportState.Done)
                    {
                        // If connection is completed, there was an unexpected disconnect
                        // TODO: uv_strerror(length)
                        _notifier.FireEvent(_transporter.Client, new UnexpectedDisconnectEvent("Read Error Or Close"), _transporter.ClientConfig.IsPollingEnabled);
                    }
                    else
                    {
                        // Otherwise, the client failed to connect.
                        // TODO: uv_strerror(length)
                        _notifier.FireEvent(_transporter.Client, new ConnectFailedEvent("Failed to complete pitaya connection"), _transporter.ClientConfig.IsPollingEnabled);
                    }
                    _onReconnect?.Invoke();
                    return;
                }

                _packetParser.Parse(state._readBuffer, 0, length);

                //Read next message
                if (_isRunning)
                    Read(state);
            }
            catch
            {

            }
        }

        private void Print(byte[] bytes, int offset, int length)
        {
            for (int i = offset; i < length; i++)
                Console.Write(Convert.ToString(bytes[i], 16) + " ");
            Console.WriteLine();
        }
    }
}
