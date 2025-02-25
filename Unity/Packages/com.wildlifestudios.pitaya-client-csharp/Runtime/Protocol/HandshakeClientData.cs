using Newtonsoft.Json;

namespace Wildlife.PitayaCSharp.Protocol
{
    // HandshakeClientData represents information about the client sent on the handshake.
    public class HandshakeClientData
    {
        [JsonProperty(PropertyName = "platform")]
        public string Platform;
        [JsonProperty(PropertyName = "libVersion")]
        public string LibVersion;
        [JsonProperty(PropertyName = "clientBuildNumber")]
        public string BuildNumber;
        [JsonProperty(PropertyName = "clientVersion")]
        public string Version;
    }
}