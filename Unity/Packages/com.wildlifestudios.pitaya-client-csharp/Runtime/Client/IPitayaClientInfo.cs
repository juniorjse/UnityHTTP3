namespace Wildlife.PitayaCSharp.Client
{
    public interface IPitayaClientInfo
    {
        /// <summary>
        /// Environment in which the application is running.
        /// </summary>
        string Platform { get; }
        /// <summary>
        /// A version identifier used when building softwares.
        /// </summary>
        string BuildNumber { get; }
        /// <summary>
        /// The current version of the application.
        /// </summary>
        string Version { get; }

    }
}
