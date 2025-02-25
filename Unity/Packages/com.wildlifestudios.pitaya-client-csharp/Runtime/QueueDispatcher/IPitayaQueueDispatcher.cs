using System;

namespace Wildlife.PitayaCSharp.QueueDispatcher
{
    public interface IPitayaQueueDispatcher
    {
        void Dispatch(Action action);
    }

}