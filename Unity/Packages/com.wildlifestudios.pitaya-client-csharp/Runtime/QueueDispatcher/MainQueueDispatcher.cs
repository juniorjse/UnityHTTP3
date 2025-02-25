using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Wildlife.PitayaCSharp.QueueDispatcher
{
    public class MainQueueDispatcher : IPitayaQueueDispatcher
    {
        public static string ExceptionErrorMessage = "Exception ocurred on dispatcher list";

        static MainQueueDispatcher _instance;

        readonly List<Action> _actions;
        readonly List<Action> _actionsCopy;
        Timer _timer;
        readonly static object Lock = new object();

        public bool ShouldThrowExceptions { get; set; }

        private MainQueueDispatcher()
        {
            _actions = new List<Action>();
            _actionsCopy = new List<Action>();
            _timer = new Timer(OnTimerElapsed, null, 0, 16); // Approximately 60 FPS (1000ms / 60)
        }

        ~MainQueueDispatcher()
        {
            _timer?.Dispose();
        }

        public static MainQueueDispatcher GetInstance(bool shouldThrowExceptions = false)
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MainQueueDispatcher
                        {
                            ShouldThrowExceptions = shouldThrowExceptions
                        };
                    }
                }
            }

            return _instance;
        }

        public void Dispatch(Action action)
        {
            lock (Lock)
            {
                _actions.Add(action);
            }
        }

        private void OnTimerElapsed(object state)
        {
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            lock (Lock)
            {
                _actionsCopy.Clear();
                _actionsCopy.AddRange(_actions);
                _actions.Clear();

                foreach (var action in _actionsCopy)
                {
                    if (ShouldThrowExceptions)
                    {
                        action();
                    }
                    else
                    {
                        SafeInvoke(action);
                    }
                }
            }
        }

        void SafeInvoke(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{ExceptionErrorMessage}: " + e);
            }
        }
    }
}