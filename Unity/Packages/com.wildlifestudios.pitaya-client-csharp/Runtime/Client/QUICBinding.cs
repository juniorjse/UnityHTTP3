using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Wildlife.PitayaCSharp.Transport;
using AOT;

internal delegate void NativeConnectionDoneCallback(string message);

internal delegate void NativeReadCallback(IntPtr readBuffer, int length);

internal delegate void NativeSendCallback(bool error);

namespace Wildlife.PitayaCSharp.Client
{
    public interface IQUICBinding
    {
        void Connect(string host, ushort port, string handshakeOpts, QUICTransporter transporter);
        void Disconnect();
        void Read();
        void Send(byte[] packetBuffer);
    }

    public class QUICBinding : IQUICBinding
    {
        public void Connect(string host, ushort port, string handshakeOpts, QUICTransporter transporter) { StaticQUICBinding.Connect(host, port, handshakeOpts, transporter); }
        public void Disconnect() { StaticQUICBinding.Disconnect(); }
        public void Read() { StaticQUICBinding.Read(); }
        public void Send(byte[] packetBuffer) { StaticQUICBinding.Send(packetBuffer); }
    }

    public static class StaticQUICBinding
    {
        private static readonly NativeConnectionDoneCallback NativeConnectionDoneCallback;
        private static readonly NativeReadCallback NativeReadCallback;
        private static readonly NativeSendCallback NativeSendCallback;

        //private static readonly Dictionary<IntPtr, WeakReference> Listeners = new Dictionary<IntPtr, WeakReference>();

        private static QUICTransporter qUICTransporter;

        static StaticQUICBinding()
        {
            NativeConnectionDoneCallback = OnConnectionDone;
            NativeReadCallback = OnRead;
            NativeSendCallback = OnSend;
        }

        public static void Connect(string host, ushort port, string handshakeOpts, QUICTransporter transporter)
        {
            var opts = string.IsNullOrEmpty(handshakeOpts) ? null : handshakeOpts;
            qUICTransporter = transporter;

            byte[] certificateBytes = File.ReadAllBytes("/Users/victor/Documents/teste/quic_jr/pitaya/pkg/acceptor/fixtures/server.der");

            using (var stream = new MemoryStream(certificateBytes))
            {
                byte[] buffer = stream.ToArray();
                IntPtr certificateBufferPtr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                uint bufferLength = (uint)buffer.Length;
                NativeConnect(host, port, certificateBufferPtr, bufferLength, NativeConnectionDoneCallback);
            }

            
        }

        public static void Disconnect()
        {
            NativeDisconnect();
        }

        public static void Read()
        {
            NativeRead(NativeReadCallback);
        }

        public static void Send(byte[] packetBuffer)
        {
            using (MemoryStream stream = new MemoryStream(packetBuffer))
            {
                byte[] buffer = stream.ToArray();
                IntPtr packetBufferPtr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                uint bufferLength = (uint)buffer.Length;
                NativeSend(packetBufferPtr, bufferLength, NativeSendCallback);
            }
        }

        //-----------------------NATIVE CALLBACKS-------------------------------//
        [MonoPInvokeCallback(typeof(NativeConnectionDoneCallback))]
        private static void OnConnectionDone(string message)
        {
            qUICTransporter.OnConnectionDone(message);
        }

        [MonoPInvokeCallback(typeof(NativeSendCallback))]
        private static void OnSend(bool error)
        {
            qUICTransporter.OnSend(error);
        }

        [MonoPInvokeCallback(typeof(NativeReadCallback))]
        private static void OnRead(IntPtr readBuffer, int length)
        {
            byte[] buffer = new byte[length];
            Marshal.Copy(readBuffer, buffer, 0, length);

            qUICTransporter.OnRead(buffer, length);
        }

#if UNITY_IOS
        private const string LibName = "__Internal";
#elif (UNITY_ANDROID) && !UNITY_EDITOR
        private const string LibName = "quic-android";
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
        private const string LibName = "quic-mac";
#elif (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
        private const string LibName = "quic-windows";
#else
        private const string LibName = "__Internal";
#endif

        [DllImport(LibName, EntryPoint = "connectQUIC")]
        private static extern void NativeConnect(string hostAddress, int port, IntPtr certificateBuffer, uint certificateBufferLength, NativeConnectionDoneCallback onConnectionDone);
        [DllImport(LibName, EntryPoint = "disconnectQUIC")]
        private static extern void NativeDisconnect();
        [DllImport(LibName, EntryPoint = "sendQUIC")]
        private static extern void NativeSend(IntPtr packetBuffer, uint packetBufferLength, NativeSendCallback onSend);
        [DllImport(LibName, EntryPoint = "readQUIC")]
        private static extern void NativeRead(NativeReadCallback onRead);
    }
}
