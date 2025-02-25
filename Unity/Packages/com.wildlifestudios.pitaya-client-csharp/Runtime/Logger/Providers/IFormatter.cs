namespace Wildlife.PitayaCSharp.Logger.Providers
{
    /// <summary>
    /// IFormatter defines the human readable format in which the logging fields will be shown.
    /// </summary>
    internal interface IFormatter
    {
        string Format(object obj);
    }
}
