using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Wildlife.PitayaCSharp.Logger
{
    /// <summary>
    /// Represents the varying content of a log message.
    /// </summary>
    [Serializable]
    public class LogMessage
    {
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime ClientTimestamp { get; set; }

        public string ErrorMessage { get; set; }

        public string StackTrace { get; set; }

        public PitayaLogLevel Level { get; set; }

        public string Message { get; set; }

        public string FunctionName { get; set; }

        public string FileName { get; set; }

        public int LineNumber { get; set; }
    }

}
