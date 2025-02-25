using System;
using System.Collections.Generic;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Event;
using Wildlife.PitayaCSharp.Error;
using Wildlife.PitayaCSharp.Serializer;

namespace Wildlife.PitayaCSharp.Transport
{
    public class TransporterNotifyService
    {
        Dictionary<IPitayaClient, IPitayaListener> _listeners = new();
        readonly object _eventHandlersLock = new();
        Dictionary<IPitayaListener, LinkedList<Action<IClientEvent>>> _eventHandlers = new();
        Dictionary<IPitayaListener, Action<string, byte[]>> _pushHandlers = new();

        internal void Subscribe(IPitayaClient client, IPitayaListener listener)
        {
            if (_listeners.ContainsKey(client))
                Unsubscribe(client);

            _listeners[client] = listener;
            _eventHandlers[listener] = new();
            _pushHandlers[listener] = (route, data) => { };
        }

        internal void Unsubscribe(IPitayaClient client)
        {
            if (_listeners.TryGetValue(client, out var listener))
            {
                _listeners.Remove(client);

                _eventHandlers.Remove(listener);
                _pushHandlers.Remove(listener);

                PitayaClientLib.Logger.LogInfo("Listener removed for the client.");
            }
            else
            {
                PitayaClientLib.Logger.LogError("No listener found for the client.");
            }
        }

        internal void AddEventHandler(IPitayaClient client, Action<IClientEvent> eventHandler)
        {
            if (_listeners.TryGetValue(client, out var listener))
            {
                _eventHandlers[listener].AddLast(eventHandler);
            }
            else
            {
                PitayaClientLib.Logger.LogError("No listener found for the client.");
            }
        }

        internal void SetPushHandler(IPitayaClient client, Action<string, byte[]> pushHandler)
        {
            if (_listeners.TryGetValue(client, out var listener))
            {
                _pushHandlers[listener] = pushHandler;
            }
            else
            {
                PitayaClientLib.Logger.LogError("No listener found for the client.");
            }
        }

        internal void FireEvent(IPitayaClient client, IClientEvent clientEvent, bool shouldPoll)
        {
            if (client == null)
            {
                PitayaClientLib.Logger.LogError("pc_client_fire_event - client is null");
                return;
            }

            if (shouldPoll)
                EnqueueEvent(_listeners[client], clientEvent);
            else
                FireEvent(_listeners[client], clientEvent);
        }

        private void EnqueueEvent(IPitayaListener listener, IClientEvent clientEvent)
        {
            // Yet to be implemented    
        }

        private void FireEvent(IPitayaListener listener, IClientEvent clientEvent)
        {
            PitayaClientLib.Logger.LogInfo(string.Format("pc__trans_fire_event - fire event: {0}, arg1: {1}, arg2: {2}", clientEvent.GetNextClientState(), clientEvent.Arg1, clientEvent.Arg2));

            /* invoke handler */
            lock (_eventHandlersLock)
            {
                if (_eventHandlers.TryGetValue(listener, out var eventHandlerList))
                {
                    foreach (Action<IClientEvent> eventHandler in eventHandlerList)
                    {
                        eventHandler.Invoke(clientEvent);
                    }
                }
            }
        }

        internal void FirePush(IPitayaClient client, string route, byte[] messageData, bool shouldPoll)
        {
            if (client == null)
            {
                PitayaClientLib.Logger.LogError("pc_trans_push - client is null");
                return;
            }

            if (shouldPoll)
                EnqueuePush(_listeners[client], route, messageData);
            else
                FirePush(_listeners[client], route, messageData);
        }

        private void EnqueuePush(IPitayaListener listener, string route, byte[] messageData)
        {
            // Yet to be implemented    
        }

        private void FirePush(IPitayaListener listener, string route, byte[] messageData)
        {
            if (messageData.Length == 0)
            {
                PitayaClientLib.Logger.LogError("pc__trans_push - empty buffer");
                return;
            }

            PitayaClientLib.Logger.LogInfo("pc__trans_push - route: " + route);

            /* invoke handler */
            _pushHandlers[listener].Invoke(route, messageData);
        }

        internal void FireSent(IPitayaClient client, uint sequenceNumber, PitayaInternalError error, bool shouldPoll)
        {
            if (client == null)
            {
                PitayaClientLib.Logger.LogError("pc_trans_sent - client is null");
                return;
            }

            if (shouldPoll)
                EnqueueSent(_listeners[client], sequenceNumber, error);
            else
                FireSent(_listeners[client], sequenceNumber, error);
        }

        private void EnqueueSent(IPitayaListener listener, uint sequenceNumber, PitayaInternalError error)
        {
            // Yet to be implemented
        }

        private void FireSent(IPitayaListener listener, uint sequenceNumber, PitayaInternalError error = null)
        {
            if (error != null)
            {
                OnNotify(sequenceNumber, error);
            }
        }

        internal void FireResponse(IPitayaClient client, uint messageId, byte[] responseData, bool shouldPoll, PitayaInternalError error = null, ProtobufSerializer.SerializationFormat serializer = default)
        {
            if (client == null)
            {
                PitayaClientLib.Logger.LogError("pc_trans_resp - client is null");
                return;
            }

            if (shouldPoll)
                EnqueueResponse(_listeners[client], messageId, responseData, error);
            else
                FireResponse(_listeners[client], messageId, responseData, error, serializer);
        }

        private void EnqueueResponse(IPitayaListener listener, uint messageId, byte[] responseData, PitayaInternalError error = null)
        {
            // Yet to be implemented    
        }

        private void FireResponse(IPitayaListener listener, uint messageId, byte[] responseData, PitayaInternalError error = null, ProtobufSerializer.SerializationFormat serializer = default)
        {
            if (error != null)
            {
                PitayaClientLib.Logger.LogInfo(string.Format("pc__trans_resp - fire resp event, req_id: {0}, error: {1}",
                       messageId, error.Code));
                /* invoke handler */
                OnError(listener, messageId, error, serializer);
            }
            else
            {
                PitayaClientLib.Logger.LogInfo(string.Format("pc__trans_resp - fire resp event, req_id: {0}", messageId));
                /* invoke handler */
                OnRequest(listener, messageId, responseData);
            }
        }

        private void OnNotify(uint sequenceNumber, PitayaInternalError error)
        {
            PitayaClientLib.Logger.LogDebug(string.Format("OnNotify | rc={0}", error));
        }

        private void OnError(IPitayaListener listener, uint messageId, PitayaInternalError internalError, ProtobufSerializer.SerializationFormat serializer)
        {
            PitayaError pitayaError;

            if (internalError is PitayaServerError serverError)
            {
                pitayaError = PitayaErrorFactory.CreatePitayaError(serverError, serializer);
            }
            else
            {
                pitayaError = new PitayaError(internalError.ToString(), "Internal Pitaya error");
            }

            PitayaClient.QueueDispatcher.Dispatch(() =>
            {
                listener.OnRequestError(messageId, pitayaError);
            });
        }

        private void OnRequest(IPitayaListener listener, uint messageId, byte[] messageData)
        {
            PitayaClient.QueueDispatcher.Dispatch(() =>
            {
                listener.OnRequestResponse(messageId, messageData);
            });
        }
    }
}