using Newtonsoft.Json;

namespace Wildlife.PitayaCSharp.Logger.Providers
{
    internal class JsonFormatter : IFormatter
    {
        public string Format(object obj) => JsonConvert.SerializeObject(obj);
    }
}
