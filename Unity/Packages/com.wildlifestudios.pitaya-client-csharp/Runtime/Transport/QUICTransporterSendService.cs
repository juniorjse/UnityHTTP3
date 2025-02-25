using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Error;

namespace Wildlife.PitayaCSharp.Transport
{
    internal class QUICTransporterSendService
    {
        public delegate void BooleanCallback(bool arg);
        public const int TimeoutCheckIntervalInSeconds = 2;

        AutoResetEvent _sleepEvent;
        bool _isRunning;
        int _timeToSleep;

        Thread _mainThread;
        bool _disposed;

        bool _sendingMessage;
        int _sendingMessageCount;
        readonly object _sendingMessageLock;
        Stream _sendStream;

        Queue<TransporterSendItem> _connectionPendingQueue;
        LinkedList<TransporterSendItem> _writeWaitQueue;
        Queue<TransporterSendItem> _writingQueue;
        LinkedList<TransporterSendItem> _responsePendingQueue;

        readonly object _writeQueueLock;
        UVTimer _checkTimeout;

        ITransporter _transporter;
        TransporterNotifyService _notifier;

        internal QUICTransporterSendService(int timeToSleep, ITransporter transporter, TransporterNotifyService notifier)
        {
            _timeToSleep = timeToSleep;
            _connectionPendingQueue = new Queue<TransporterSendItem>();
            _writeWaitQueue = new LinkedList<TransporterSendItem>();
            _writingQueue = new Queue<TransporterSendItem>();
            _responsePendingQueue = new LinkedList<TransporterSendItem>();
            _writeQueueLock = new object();

            _transporter = transporter;
            _notifier = notifier;

            _checkTimeout = new UVTimer();

            _sendingMessage = false;
            _sendingMessageCount = 0;
            _sendingMessageLock = new object();

            _sleepEvent = new AutoResetEvent(false);

            _mainThread = new Thread(RunMainThread);
            _mainThread.IsBackground = false;
            _mainThread.Priority = ThreadPriority.Normal;
            _mainThread.Name = "TransporterSend_" + DateTime.Now.Ticks;

            _disposed = false;
            _isRunning = false;
        }

        ~QUICTransporterSendService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Start(Stream sendStream = null)
        {
            _sendStream = sendStream;
            _isRunning = true;
            _mainThread.Start();
        }

        internal void PutItemToWriteWaitQueue(TransporterSendItem sendItem)
        {
            PacketType packetType = PacketProtocol.GetPacketTypeFromHeader(sendItem.Buffer);

            lock (_writeQueueLock)
            {
                if (packetType == PacketType.Handshake || packetType == PacketType.HandshakeAck)
                    /*
                    * insert to head, because handshake ack/req should be sent
                    * before any application data.
                    */
                    _writeWaitQueue.AddFirst(sendItem);
                else
                    _writeWaitQueue.AddLast(sendItem);
            }
        }

        internal void PutItemToConnectionPendingQueue(TransporterSendItem sendItem)
        {
            lock (_writeQueueLock)
            {
                _connectionPendingQueue.Enqueue(sendItem);
            }
        }

        internal void RemoveRequestFromResponsePendingQueue(uint messageUid)
        {
            var currentNode = _responsePendingQueue.First;

            while (currentNode != null)
            {
                if (currentNode.Value.RequestUid == messageUid)
                {
                    var nodeToRemove = currentNode;
                    _responsePendingQueue.Remove(nodeToRemove); // O(1) according to MS
                    break;
                }

                currentNode = currentNode.Next;
            }
        }


        internal void Send()
        {
            if (_isRunning)
            {
                _sleepEvent.Set();
            }
        }

        void RunMainThread()
        {
            _sleepEvent.WaitOne(_timeToSleep);

            int buffersCount;
            bool needCheck;
            byte[][] buffers;

            while (_isRunning)
            {
                try
                {
                    if (!_sendingMessage)
                    {
                        needCheck = false;
                        buffers = new byte[0][];
                        lock (_writeQueueLock)
                        {

                            while (_connectionPendingQueue.Count > 0)
                            {
                                var sendItem = _connectionPendingQueue.Dequeue();

                                if (sendItem.Type != TransporterSendItemType.Internal)
                                {
                                    PitayaClientLib.Logger.LogDebug(string.Format("tcp__write_async_cb - move wi from conn pending to write wait queue," +
                                        "seq_num: {0}, req_id: {1}", sendItem.SequenceNumber, sendItem.RequestUid));
                                }

                                _writeWaitQueue.AddLast(sendItem);
                            }

                            buffersCount = 0;

                            foreach (var sendItem in _writeWaitQueue)
                            {
                                if (sendItem.Type != TransporterSendItemType.Internal && sendItem.Timeout != -1)
                                {
                                    needCheck = true;
                                }

                                buffersCount++;
                            }

                            if (buffersCount > 0)
                            {
                                buffers = new byte[buffersCount][];
                                uint i = 0;

                                while (_writeWaitQueue.Count > 0)
                                {
                                    TransporterSendItem sendItem = _writeWaitQueue.First.Value;
                                    _writeWaitQueue.RemoveFirst();

                                    if (sendItem.Type != TransporterSendItemType.Internal)
                                    {
                                        PitayaClientLib.Logger.LogDebug(string.Format("tcp__write_async_cb - move wi from write wait to writing queue," +
                                            "seq_num: {0}, req_id: {1}", sendItem.SequenceNumber, sendItem.RequestUid));
                                    }

                                    buffers[i++] = sendItem.Buffer;

                                    _writingQueue.Enqueue(sendItem);
                                }
                            }
                        }

                        if (buffersCount == 0)
                        {
                            if (needCheck)
                            {
                                PitayaClientLib.Logger.LogDebug("WEE NEED A CHEECK");
                                /* if there are pending req, we should start to check timeout */
                                if (!_checkTimeout.IsActive())
                                {
                                    PitayaClientLib.Logger.LogDebug("tcp__write_async_cb - start check timeout timer");
                                    _checkTimeout.Start(OnWriteCheckTimeout, null,
                                            TimeoutCheckIntervalInSeconds * 1000);
                                }
                                PitayaClientLib.Logger.LogDebug("tcp__write_async_cb - start check timeout timer");
                            }
                        }
                        else
                        {
                            PitayaClientLib.Logger.LogDebug("tcp__write_async_cb - Writing to QUIC socket");
                            foreach (byte[] buf in buffers)
                            {
#if UNITY_IOS
                                StaticQUICBinding.Send(buf);
#endif
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    PitayaClientLib.Logger.LogError("tcp__write_async_cb - uv write error: " + e.Message);

                    lock (_writeQueueLock)
                    {
                        while (_writingQueue.Count > 0)
                        {
                            var sendItem = _writingQueue.Dequeue();

                            if (sendItem.Type == TransporterSendItemType.Notify)
                            {
                                PitayaInternalError error = new PitayaUVError(5);
                                _notifier.FireSent(_transporter.Client, sendItem.SequenceNumber, error, _transporter.ClientConfig.IsPollingEnabled);
                            }

                            if (sendItem.Type == TransporterSendItemType.Response)
                            {
                                PitayaInternalError error = new PitayaUVError(5);
                                byte[] emptyBuffer = new byte[0];
                                _notifier.FireResponse(_transporter.Client, sendItem.RequestUid, emptyBuffer, _transporter.ClientConfig.IsPollingEnabled, error, _transporter.Serializer.Value);
                            }

                            /* if internal, do nothing here. */

                            //pre_alloc not implemented
                        }
                    }
                }
                finally
                {
                    if (_isRunning)
                    {
                        _sleepEvent.WaitOne(_timeToSleep);
                    }
                }
            }
        }

        public void OnSend(bool error)
        {
            if (_isRunning)
            {
                lock (_writeQueueLock)
                {
                    while (_writingQueue.Count > 0)
                    {
                        var sendItem = _writingQueue.Dequeue();

                        if (!error && sendItem.Type == TransporterSendItemType.Response)
                        {
                            PitayaClientLib.Logger.LogDebug(string.Format("quic__write_done_cb - move wi from writing to resp pending queue,"
                            + " req_id: {0}", sendItem.RequestUid));

                            _responsePendingQueue.AddLast(sendItem);
                            continue;
                        }

                        if (sendItem.Type == TransporterSendItemType.Notify)
                        {
                            PitayaInternalError err = error ? new PitayaUVError(5) : null;
                            _notifier.FireSent(_transporter.Client, sendItem.SequenceNumber, err, _transporter.ClientConfig.IsPollingEnabled);
                        }

                        if (error && sendItem.Type == TransporterSendItemType.Response)
                        {
                            PitayaInternalError err = new PitayaUVError(5);
                            byte[] emptyBuffer = new byte[0];
                            _notifier.FireResponse(_transporter.Client, sendItem.RequestUid, emptyBuffer, _transporter.ClientConfig.IsPollingEnabled, err, _transporter.Serializer.Value);
                        }
                        /* if internal, do nothing here. */

                        // pre_alloc not implemented
                    }
                }

                Send();
            }
        }

        bool CheckQueueTimeout(Queue<TransporterSendItem> items, bool needAnotherCheck)
        {
            Queue<TransporterSendItem> tmp;
            DateTime currentTime = DateTime.Now;

            tmp = new Queue<TransporterSendItem>();

            while (items.Count > 0)
            {
                var sendItem = items.Dequeue();
                if (sendItem.Timeout != -1)
                {
                    if (currentTime > sendItem.Timestamp.AddMilliseconds(sendItem.Timeout))
                    {
                        if (sendItem.Type == TransporterSendItemType.Notify)
                        {
                            PitayaClientLib.Logger.LogWarning("tcp__check_queue_timeout - notify timeout, seq num: " + sendItem.SequenceNumber);
                            PitayaInternalError err = new PitayaTimeoutError();
                            _notifier.FireSent(_transporter.Client, sendItem.SequenceNumber, err, _transporter.ClientConfig.IsPollingEnabled);
                        }
                        else if (sendItem.Type == TransporterSendItemType.Response)
                        {
                            PitayaClientLib.Logger.LogWarning("tcp__check_queue_timeout - request timeout, req id: " + sendItem.RequestUid);
                            PitayaInternalError err = new PitayaTimeoutError();
                            byte[] emptyBuffer = new byte[0];
                            _notifier.FireResponse(_transporter.Client, sendItem.RequestUid, emptyBuffer, _transporter.ClientConfig.IsPollingEnabled, err, _transporter.Serializer.Value);
                        }

                        /* if internal, just drop it. */

                        //pre_alloc not implemented

                        continue;
                    }
                    else
                    {
                        /*
                        * continue to check timeout next tick
                        * if there are wis has timeout configured but not triggered this time.
                        */
                        needAnotherCheck = true;
                    }
                }
                /* add the non-timeout wi to queue tmp */
                tmp.Enqueue(sendItem);
            }

            items = tmp;
            return needAnotherCheck;
        }

        bool CheckQueueTimeout(LinkedList<TransporterSendItem> items, bool needAnotherCheck)
        {
            LinkedList<TransporterSendItem> tmp;
            DateTime currentTime = DateTime.Now;

            tmp = new LinkedList<TransporterSendItem>();

            while (items.Count > 0)
            {
                var sendItem = items.First.Value;
                items.RemoveFirst();
                if (sendItem.Timeout != -1)
                {
                    if (currentTime > sendItem.Timestamp.AddMilliseconds(sendItem.Timeout))
                    {
                        if (sendItem.Type == TransporterSendItemType.Notify)
                        {
                            PitayaClientLib.Logger.LogWarning("tcp__check_queue_timeout - notify timeout, seq num: " + sendItem.SequenceNumber);
                            PitayaInternalError err = new PitayaTimeoutError();
                            _notifier.FireSent(_transporter.Client, sendItem.SequenceNumber, err, _transporter.ClientConfig.IsPollingEnabled);
                        }
                        else if (sendItem.Type == TransporterSendItemType.Response)
                        {
                            PitayaClientLib.Logger.LogWarning("tcp__check_queue_timeout - request timeout, req id: " + sendItem.RequestUid);
                            PitayaInternalError err = new PitayaTimeoutError();
                            byte[] emptyBuffer = new byte[0];
                            _notifier.FireResponse(_transporter.Client, sendItem.RequestUid, emptyBuffer, _transporter.ClientConfig.IsPollingEnabled, err, _transporter.Serializer.Value);
                        }

                        /* if internal, just drop it. */

                        //pre_alloc not implemented

                        continue;
                    }
                    else
                    {
                        /*
                        * continue to check timeout next tick
                        * if there are wis has timeout configured but not triggered this time.
                        */
                        needAnotherCheck = true;
                    }
                }
                /* add the non-timeout wi to queue tmp */
                tmp.AddLast(sendItem);
            }

            items = tmp;
            return needAnotherCheck;
        }

        void OnWriteCheckTimeout(object state)
        {
            _checkTimeout.Stop();
            _checkTimeout = new UVTimer();

            bool needAnotherCheck = false;

            PitayaClientLib.Logger.LogDebug("tcp__write_check_timeout_cb - start to check timeout");

            lock (_writeQueueLock)
            {
                needAnotherCheck = CheckQueueTimeout(_connectionPendingQueue, needAnotherCheck);
                needAnotherCheck = CheckQueueTimeout(_writeWaitQueue, needAnotherCheck);
                needAnotherCheck = CheckQueueTimeout(_responsePendingQueue, needAnotherCheck);
            }

            if (needAnotherCheck && !_checkTimeout.IsActive())
            {
                _checkTimeout.Start(OnWriteCheckTimeout, null,
                        TimeoutCheckIntervalInSeconds * 1000);
            }

            PitayaClientLib.Logger.LogDebug("tcp__write_check_timeout_cb - finish to check timeout");
        }

        void Dispose(bool disposing)
        {
            _isRunning = false;

            if (!_disposed)
            {
                _disposed = true;

                _sleepEvent.Set();

                if (disposing)
                {
                    //HACK
                    //NetStandard 2.0 and .Net 4.X doesnt' have Clear method as .Net Standard 2.1 and .Net Core 5.0 have :(
                    //_dataToSend.Clear();
                    _connectionPendingQueue = new Queue<TransporterSendItem>();
                    _writeWaitQueue = new LinkedList<TransporterSendItem>();
                    _writingQueue = new Queue<TransporterSendItem>();
                    _responsePendingQueue = new LinkedList<TransporterSendItem>();
                }

                if (Thread.CurrentThread != _mainThread
                    && _mainThread != null)
                {
                    while (_mainThread != null
                           && _mainThread.ThreadState != ThreadState.Aborted
                           && _mainThread.ThreadState != ThreadState.Stopped
                           && _mainThread.ThreadState != ThreadState.Unstarted)
                    {
                        Thread.Sleep(250);
                    }

                    _mainThread = null;
                }

                if (_sendStream != null)
                {
                    _sendStream.Close();
                    _sendStream.Dispose();
                    _sendStream = null;
                }

                _sleepEvent.Dispose();
            }
        }
    }
}
