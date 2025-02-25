using System;
using System.Timers;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Transport;

namespace Wildlife.PitayaCSharp.Protocol
{
    public class HeartbeatService
    {
        int _intervalInMilliseconds;
        public int Timeout;
        const int _timeoutFactor = 4;
        System.Timers.Timer _timer;
        DateTime _lastServerPacketTime;
        DateTime _heartbeatLastSentAt;
        Action<DateTime, DateTime, int> _callback;

        ITransporter _transporter;

        public HeartbeatService(int interval, ITransporter transporter, Action<DateTime, DateTime, int> callback = null)
        {
            _intervalInMilliseconds = interval * 1000;
            _transporter = transporter;
            _callback = callback;
        }

        internal void ResetTimeout()
        {
            Timeout = 0;
            _lastServerPacketTime = DateTime.Now;
        }

        internal void InvokeCallback()
        {
            _callback?.Invoke(_heartbeatLastSentAt, _lastServerPacketTime, _intervalInMilliseconds);
        }

        private void OnTimerElapsed(object source, ElapsedEventArgs e)
        {
            TimeSpan timeElapsed = DateTime.Now - _lastServerPacketTime;
            Timeout = (int)timeElapsed.TotalMilliseconds;
            int threshold = _intervalInMilliseconds * _timeoutFactor;

            //check timeout
            if (Timeout > threshold)
            {
                PitayaClientLib.Logger.LogWarning("tcp__heartbeat_timer_cb - heartbeat timeout, will reconn");
                Stop();
                _transporter.Client.Disconnect();
                return;
            }

            //Send heart beat
            PitayaClientLib.Logger.LogDebug("tcp__send__heartbeat - send heartbeat");

            byte[] packet = PacketProtocol.Encode(PacketType.Heartbeat, new byte[0]);
            _transporter.Send(packet, uint.MaxValue, uint.MaxValue, -1);
            _heartbeatLastSentAt = DateTime.Now; // TODO: Here it's not sent yet, just added to the transporter queue, maybe must change the SendCallback signature on TransporterSend
        }

        internal void Start()
        {
            if (_intervalInMilliseconds < 1000) return;

            //start heartbeat
            _timer = new System.Timers.Timer();
            _timer.Interval = _intervalInMilliseconds;
            _timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            _timer.Enabled = true;

            //Set timeout
            Timeout = 0;
            _lastServerPacketTime = DateTime.Now;

            // HACK: As it turns out, connection seems to send a heartbeat message to the server, so this
            // fix the problem of getting a negative value for rtt later on.
            _heartbeatLastSentAt = DateTime.Now;
        }

        internal void Stop()
        {
            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Dispose();
            }
        }
    }
}
