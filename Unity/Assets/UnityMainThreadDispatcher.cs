using System;
using UnityEngine;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    public void Update()
    {
        while (_executionQueue.Count > 0)
        {
            Action action = null;
            lock (_executionQueue)
            {
                action = _executionQueue.Dequeue();
            }
            action?.Invoke();
        }
    }
}
