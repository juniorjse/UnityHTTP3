using System;
using Wildlife.PitayaCSharp.Logger;
using Wildlife.PitayaCSharp.Logger.Providers;
using Wildlife.PitayaCSharp.Transport;

namespace Wildlife.PitayaCSharp.Client
{
    public static class PitayaClientLib
    {
        public static IPitayaClientInfo Info { get; private set; }

        public static IPitayaLogger Logger { get; private set; }

        internal static PitayaLogLevel _defaultLogLevel = PitayaLogLevel.Disable;

        private static void DefaultLogFunction(PitayaLogLevel level, string message)
        {
            if (level < _defaultLogLevel) return;

            DateTime currentTime = DateTime.Now;
            string messageWithTimestamp = currentTime.ToString("[yyyy-MM-dd HH:mm:ss] ") + message;

            switch (level)
            {
                case PitayaLogLevel.Debug:
                    Console.WriteLine("[DEBUG] " + messageWithTimestamp);
                    break;
                case PitayaLogLevel.Info:
                    Console.WriteLine("[INFO] " + messageWithTimestamp);
                    break;
                case PitayaLogLevel.Warn:
                    Console.WriteLine("[WARN] " + messageWithTimestamp);
                    break;
                case PitayaLogLevel.Error:
                    Console.WriteLine("[ERROR] " + messageWithTimestamp);
                    break;
                case PitayaLogLevel.Fatal:
                    Console.WriteLine("[FATAL] " + messageWithTimestamp);
                    break;
                case PitayaLogLevel.Disable:
                    return;
                default:
                    Console.WriteLine("[DEBUG] " + messageWithTimestamp);
                    break;
            }
        }

        public static void Init(LogFunction pitayaClientLogFunction = null, IPitayaClientInfo info = null)
        {
            pitayaClientLogFunction ??= DefaultLogFunction;
            Logger = new PitayaLogger(_defaultLogLevel, pitayaClientLogFunction);

            info ??= new PitayaClientInfo();
            Info = info;

            TransporterPluginRepository pluginRepository = TransporterPluginRepository.Instance;

            pluginRepository.Register(TCPTransporterPlugin.Instance);
            Logger.LogInfo("pc_lib_init - register tcp plugin");

            pluginRepository.Register(TLSTransporterPlugin.Instance);
            Logger.LogInfo("pc_lib_init - register tls plugin");

            pluginRepository.Register(QUICTransporterPlugin.Instance);
            Logger.LogInfo("pc_lib_init - register quic plugin");
        }

        public static void SetLogLevel(PitayaLogLevel level)
        {
            _defaultLogLevel = level;
            if (Logger != null)
                Logger.LogLevel = level;
        }

        public static void SetLogFunction(LogFunction logFunction)
        {
            try
            {
                Logger = new PitayaLogger(_defaultLogLevel, logFunction);
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Cannot initialize log function");
            }
        }
        public static int GetVersionNumber() { return PitayaClientVersion.ToNumber(); }
        public static string GetVersionString() { return PitayaClientVersion.ToString(); }
    }
}
