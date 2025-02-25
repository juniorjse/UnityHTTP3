using System;

namespace Wildlife.PitayaCSharp.QueueDispatcher
{
    public class NullPitayaQueueDispatcher : IPitayaQueueDispatcher
    {
        public void Dispatch(Action action)
        {
            action();
        }
    }
    
}