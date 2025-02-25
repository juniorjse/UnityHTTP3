using Wildlife.PitayaCSharp.Transport;

namespace Wildlife.PitayaCSharp.Client
{
    public interface IPitayaClientConfig
    {
        /// <summary>
        /// Time interval in seconds for a connection to timeout.
        /// </summary>
        int ConnectionTimeoutInMilliseconds { get; }
        /// <summary>
        /// Flag for enabling automatic reconnection attempts when failing to connect or disconnecting due to errors.
        /// </summary>
        bool IsAutomaticReconnectionEnabled { get; }
        /// <summary>
        /// Number of times that connection will be re-attempted after the initial attempt fails.
        /// </summary>
        int ReconnectionMaxRetries { get; }

        int ReconnectionDelayInSeconds { get; }
        int ReconnectionMaxDelayInSeconds { get; }

        /// <summary>
        /// The retry interval to use in the exponential backoff algorithm for retrying failed requests, in seconds.
        ///
        /// For example, if set to 1 second, the service will wait 1 * 1 second to retry the request for the first time,
        /// then 1 * 2 seconds, then 1 * 4 seconds, and so on, until <see cref="ExponentialBackoffMaxAttempts"/> is
        /// reached.
        ///
        /// Defaults to 1 second.
        /// </summary>
        int ReconnectionExponentialBackoffIntervalInSeconds { get; }
        bool IsPollingEnabled { get; }
        TransporterName ClientTransporterName { get; }
        bool ShouldDisableCompression { get; }

    }
}
