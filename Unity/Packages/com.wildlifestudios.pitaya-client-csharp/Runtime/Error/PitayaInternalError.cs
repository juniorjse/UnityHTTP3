using System;

namespace Wildlife.PitayaCSharp.Error
{
    public class PitayaInternalError : Exception
    {
        public int Code { get; private set; }
        public byte[] Buffer { get; private set; }
        public int Uvcode { get; private set; }

        public PitayaInternalError(int code, byte[] message, int uvcode = default) : base(System.Text.Encoding.UTF8.GetString(message))
        {
            Code = code;
            Buffer = message;
            Uvcode = uvcode;
        }
    }

    public class PitayaUVError : PitayaInternalError
    {
        public static readonly int ErrorCode = -13;
        public PitayaUVError(int uvcode) : base(ErrorCode, new byte[0], uvcode) { }
        public override string ToString() { return "PC_RC_UV_ERROR"; }
    }

    public class PitayaServerError : PitayaInternalError
    {
        public static readonly int ErrorCode = -12;
        public PitayaServerError(byte[] payload) : base(ErrorCode, payload) { }
        public override string ToString() { return "PC_RC_SERVER_ERROR"; }
    }

    public class PitayaTimeoutError : PitayaInternalError
    {
        public static readonly int ErrorCode = -2;
        public PitayaTimeoutError() : base(ErrorCode, new byte[0]) { }
        public override string ToString() { return "PC_RC_TIMEOUT"; }
    }

    public class PitayaResetError : PitayaInternalError
    {
        public static readonly int ErrorCode = -11;
        public PitayaResetError() : base(ErrorCode, new byte[0]) { }
        public override string ToString() { return "PC_RC_RESET"; }
    }

}