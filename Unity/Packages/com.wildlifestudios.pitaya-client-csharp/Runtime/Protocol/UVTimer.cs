using System;
using System.Threading;

namespace Wildlife.PitayaCSharp.Protocol
{
    public class UVTimer
    {
        Timer _timer;
        DateTime _lastStartTime;
        CancellationTokenSource _cancellationTokenSource;

        public void Start(TimerCallback timerCallback, object? state, int dueTime, int period = Timeout.Infinite)
        {
            Stop();

            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new Timer(timerCallback, state, dueTime, period);
            _lastStartTime = DateTime.Now;
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
        }

        public bool WasStopped()
        {
            return _cancellationTokenSource.Token.IsCancellationRequested;
        }

        public bool IsActive()
        {
            return _cancellationTokenSource != null && !WasStopped();
        }

        public TimeSpan ElapsedTimeAfterStart()
        {
            var elapsed = (DateTime.Now - _lastStartTime).TotalMilliseconds;

            return TimeSpan.FromMilliseconds(elapsed);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
