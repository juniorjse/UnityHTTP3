public static class PitayaClientVersion //0.2.0
{
    public const int Major = 0;
    public const int Minor = 2;
    public const int Revision = 0;
    public const string Suffix = ""; // "alpha.N" or "beta.N", etc.

    public static int ToNumber()
    {
        return (Major * 10000) + (Minor * 100) + Revision;
    }

    public static string ToString()
    {
        return $"{Major}.{Minor}.{Revision}{(string.IsNullOrEmpty(Suffix) ? "" : "-" + Suffix)}";
    }
}