using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Wildlife.PitayaCSharp.Logger
{
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter), typeof(UpperCaseNamingStrategy))]
    public enum PitayaLogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4,
        Disable = 5
    }

    internal class UpperCaseNamingStrategy : CamelCaseNamingStrategy
    {
        protected override string ResolvePropertyName(string name) => name.ToUpper();
    }
}