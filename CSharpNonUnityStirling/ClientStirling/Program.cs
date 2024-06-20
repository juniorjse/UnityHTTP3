using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Org.BouncyCastle.Asn1.Cms;
using StirlingLabs.MsQuic.Bindings;
using StirlingLabs.Utilities;
using static StirlingLabs.MsQuic.Bindings.MsQuic;

namespace StirlingLabs.MsQuic.Tests;

public class Program
{
    private static ushort _lastPort = 8080;
    private static QuicCertificate _cert;

    private static readonly bool IsContinuousIntegration = Common.Init
        (() => (Environment.GetEnvironmentVariable("CI") ?? "").ToUpperInvariant() == "TRUE");

    // public void OneTimeSetUp()
    // {
    //     if (IsContinuousIntegration)
    //         Trace.Listeners.Add(new ConsoleTraceListener());

    //     var asmDir = Path.GetDirectoryName(new Uri(typeof(RoundTripTests).Assembly.Location).LocalPath);
    //     var p12Path = Path.Combine(asmDir!, "localhost.p12");

    //     _cert = new(File.OpenRead(p12Path));
    // }

    // public void OneTimeTearDown()
    //     => _cert.Free();

    public static void Main()
    {
        if (IsContinuousIntegration)
            Trace.Listeners.Add(new ConsoleTraceListener());

        // var asmDir = Path.GetDirectoryName(new Uri(typeof(RoundTripTests).Assembly.Location).LocalPath);
        // var p12Path = Path.Combine(asmDir!, "localhost.p12");

        _cert = new(File.OpenRead("/Users/junior/UnityHTTP3/CSharpNonUnityStirling/ClientStirling/localhost.p12"));

        Console.WriteLine($"=== SETUP Test ===");

        ushort _port = _lastPort += 1;
        Console.WriteLine(_port);
        var testName = "Test";

        QuicRegistration _reg = new(testName);

        QuicServerConfiguration _listenerCfg = new(_reg, "test");

        _listenerCfg.ConfigureCredentials(_cert);

        QuicListener _listener = new(_listenerCfg);

        _listener.Start(new(IPAddress.IPv6Loopback, _port));

        _listener.UnobservedException += (_, info) =>
        {
            info.Throw();
        };

        QuicServerConnection? _serverSide;
        _listener.NewConnection += (_, connection) =>
        {
            Console.WriteLine("handling _listener.NewConnection");
            _serverSide = connection;
            connection.CertificateReceived += (_, _, _, _, _)
                =>
            {
                Console.WriteLine("handled server CertificateReceived");
                // TODO: cheap cert validation tests
                return QUIC_STATUS_SUCCESS;
            };
            Console.WriteLine("handled _listener.NewConnection");
        };

        _listener.ClientConnected += (_, connection) =>
        {
            _serverSide = connection;
            _serverSide.UnobservedException += (_, info) =>
            {
                info.Throw();
            };
        };

        QuicClientConfiguration _clientCfg = new(_reg, "test");

        _clientCfg.ConfigureCredentials();

        Memory<byte> _ticket = null;
        QuicClientConnection _clientSide;
        // get resumption ticket
        {
            _clientSide = new(_clientCfg);

            _clientSide.ResumptionTicketReceived += c =>
            {
                _ticket = c.ResumptionTicket;
            };

            // connection
            _clientSide.CertificateReceived += (peer, certificate, chain, flags, status)
                =>
            {
                // TODO: cheap cert validation tests
                return QUIC_STATUS_SUCCESS;
            };

            _clientSide.UnobservedException += (_, info) =>
            {
                info.Throw();
            };

            CountdownEvent cdee = new CountdownEvent(2);

            _clientSide.Connected += _ =>
            {
                cdee.Signal();
            };
            _clientSide.ResumptionTicketReceived += _ =>
            {
                cdee.Signal();
            };

            _clientSide.Start("127.0.0.1", _port);

            cdee.Wait();

            _clientSide.Close();

            _clientSide.Dispose();
        }

        // setup connection to resume
        {

            _clientSide = new(_clientCfg);

            _clientSide.SetResumptionTicket(_ticket);

            // connection
            _clientSide.CertificateReceived += (peer, certificate, chain, flags, status)
                =>
            {
                // TODO: cheap cert validation tests
                return QUIC_STATUS_SUCCESS;
            };

            _clientSide.UnobservedException += (_, info) =>
            {
                info.Throw();
            };
        }

        Console.WriteLine($"=== BEGIN Test ===");

        // stream round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("oi");
        var dataLength = utf8Hello.Length;

        CountdownEvent cde = new CountdownEvent(2);

        using var clientStream = _clientSide.OpenStream(true);

        Debug.Assert(!clientStream.IsStarted);

        var streamOpened = false;
        var was0Rtt = false;

        QuicStream serverStream = null!;

        int read = 0;
        Span<byte> dataReceived = stackalloc byte[dataLength];

        // fixed (byte* pDataReceived = dataReceived)
        // {
        //     var ptrDataReceived = (IntPtr)pDataReceived;

        _listener.ClientConnected += (_, connection) =>
        {
            _serverSide = connection;
            _serverSide.UnobservedException += (_, info) =>
            {
                info.Throw();
            };
            _serverSide.IncomingStream += (_, stream) =>
            {
                serverStream = stream;
                serverStream.DataReceived += x =>
                {
                    was0Rtt = (x.LastReceiveFlags & QUIC_RECEIVE_FLAGS.ZERO_RTT) != 0;

                    // ReSharper disable once VariableHidesOuterVariable
                    var dataReceived = new Span<byte>(new byte[dataLength]);
                    read = serverStream.Receive(dataReceived);
                    Console.WriteLine("read: ", read);
                    cde.Signal();
                };
                streamOpened = true;
                cde.Signal();
            };
        };

        clientStream.Start();
        _clientSide.Start("localhost", _port);

        cde.Wait();
        // }
    }
}