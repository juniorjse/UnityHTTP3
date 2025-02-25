using Wildlife.PitayaCSharp.Transport;

namespace Wildlife.PitayaCSharp.Client
{
    public sealed class DefaultPitayaClientConfig : IPitayaClientConfig
    {
        public int ConnectionTimeoutInMilliseconds { get; }

        public bool IsAutomaticReconnectionEnabled { get; }

        public int ReconnectionMaxRetries { get; }

        public int ReconnectionDelayInSeconds { get; }

        public int ReconnectionMaxDelayInSeconds { get; }

        public int ReconnectionExponentialBackoffIntervalInSeconds { get; }

        public bool IsPollingEnabled { get; }

        public TransporterName ClientTransporterName { get; }

        public bool ShouldDisableCompression { get; }

        public DefaultPitayaClientConfig(int connectionTimeoutInSeconds = PitayaClient.DefaultConnectionTimeout, bool isAutomaticReconnectionEnabled = true,
        int reconnectionMaxRetries = 5, int reconnectionDelayInSeconds = 2, int reconnectionMaxDelayInSeconds = 30,
        int reconnectionExponentialBackoffIntervalInSeconds = 1, bool isPollingEnabled = false, TransporterName clientTransporterName = TransporterName.TCP,
        bool shouldDisableCompression = false)
        {
            ConnectionTimeoutInMilliseconds = connectionTimeoutInSeconds * 1000;
            IsAutomaticReconnectionEnabled = isAutomaticReconnectionEnabled;
            ReconnectionMaxRetries = reconnectionMaxRetries;
            ReconnectionDelayInSeconds = reconnectionDelayInSeconds;
            ReconnectionMaxDelayInSeconds = reconnectionMaxDelayInSeconds;
            ReconnectionExponentialBackoffIntervalInSeconds = reconnectionExponentialBackoffIntervalInSeconds;
            IsPollingEnabled = isPollingEnabled;
            ClientTransporterName = clientTransporterName;
            ShouldDisableCompression = shouldDisableCompression;
        }
    }
}
