namespace Wildlife.PitayaCSharp.Error
{
    public class NetworkError
    {
        public readonly string Error;
        public readonly string Description;

        public NetworkError(string error, string description)
        {
            Error = error;
            Description = description;
        }
    }
}