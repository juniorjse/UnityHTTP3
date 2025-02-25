using System.Runtime.InteropServices;
using System.Reflection;

namespace Wildlife.PitayaCSharp.Client
{
    public sealed class PitayaClientInfo : IPitayaClientInfo
    {
        public string Platform { get; }

        public string BuildNumber { get; }

        public string Version { get; }

        public PitayaClientInfo()
        {
            Platform = DeterminePlatform();
            BuildNumber = DetermineBuildNumber();
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (Version == "0.0.0.0") // default
                Version = "0.1"; // libPitaya default
        }

        private string DeterminePlatform() // TODO: maybe check for Android and iOS?
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "windows";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "mac";
            else
                return "unknown";
        }

        private string DetermineBuildNumber() // TODO: get specific BuildNumber for Android?
        {
            return "1";
        }
    }
}
