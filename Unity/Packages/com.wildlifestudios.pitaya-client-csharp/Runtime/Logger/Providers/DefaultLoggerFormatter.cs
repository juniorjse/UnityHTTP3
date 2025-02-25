using Newtonsoft.Json;

namespace Wildlife.PitayaCSharp.Logger.Providers
{
    internal class DefaultLoggerFormatter : IFormatter
    {
        bool enableHumanReadableOutput;

        internal DefaultLoggerFormatter(bool enableHumanReadableOutput = true) =>
            this.enableHumanReadableOutput = enableHumanReadableOutput;

        public string Format(object obj)
        {
            if (obj is LogMessage lm)
            {
                return enableHumanReadableOutput ? $"{lm.Message}\n{JsonConvert.SerializeObject(lm, Formatting.Indented)}" : JsonConvert.SerializeObject(lm);
            }
            return JsonConvert.SerializeObject(obj);
        }
    }
}
