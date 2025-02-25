using Newtonsoft.Json;
using System.Collections.Generic;

namespace Wildlife.PitayaCSharp.Protocol
{
    // HandshakeData represents information about the handshake sent by the client.
    // `sys` corresponds to information independent from the app and `user` information
    // that depends on the app and is customized by the user.
    public class SessionHandshakeData
    {
        [JsonProperty(PropertyName = "sys")]
        public HandshakeClientData Sys { get; set; }
        [JsonProperty(PropertyName = "user")]
        public Dictionary<string, object> User { get; set; }
    }
}