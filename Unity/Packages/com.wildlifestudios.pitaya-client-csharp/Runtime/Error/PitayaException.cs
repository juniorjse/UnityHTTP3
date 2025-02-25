using System;
using System.Collections.Generic;

namespace Wildlife.PitayaCSharp.Error
{
    public class PitayaException : Exception
    {
        public int Code { get; }

        public PitayaException(int code) : base()
        {
            Code = code;
        }

        public PitayaException(int code, string message) : base(message)
        {
            Code = code;
        }
    }

    public class PitayaCommonErrorException : PitayaException
    {
        public static readonly int ErrorCode = -1;
        public PitayaCommonErrorException() : base(ErrorCode) { }
        public PitayaCommonErrorException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_ERROR";
        }
    }

    public class PitayaInvalidJsonException : PitayaException
    {
        public static readonly int ErrorCode = -3;
        public PitayaInvalidJsonException() : base(ErrorCode) { }
        public PitayaInvalidJsonException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_INVALID_JSON";
        }
    }

    public class PitayaInvalidArgumentException : PitayaException
    {
        public static readonly int ErrorCode = -4;
        public PitayaInvalidArgumentException() : base(ErrorCode) { }
        public PitayaInvalidArgumentException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_INVALID_ARG";
        }
    }

    public class PitayaNoTransporterException : PitayaException
    {
        public static readonly int ErrorCode = -5;
        public PitayaNoTransporterException() : base(ErrorCode) { }
        public PitayaNoTransporterException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_NO_TRANS";
        }
    }

    public class PitayaInvalidThreadException : PitayaException
    {
        public static readonly int ErrorCode = -6;
        public PitayaInvalidThreadException() : base(ErrorCode) { }
        public PitayaInvalidThreadException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_INVALID_THREAD";
        }
    }

    public class PitayaTransporterErrorException : PitayaException
    {
        public static readonly int ErrorCode = -7;
        public PitayaTransporterErrorException() : base(ErrorCode) { }
        public PitayaTransporterErrorException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_TRANS_ERROR";
        }
    }

    public class PitayaInvalidRouteException : PitayaException
    {
        public static readonly int ErrorCode = -8;
        public PitayaInvalidRouteException() : base(ErrorCode) { }
        public PitayaInvalidRouteException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_INVALID_ROUTE";
        }
    }

    public class PitayaInvalidStateException : PitayaException
    {
        public static readonly int ErrorCode = -9;
        public PitayaInvalidStateException() : base(ErrorCode) { }
        public PitayaInvalidStateException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_INVALID_STATE";
        }
    }

    public class PitayaNotFoundException : PitayaException
    {
        public static readonly int ErrorCode = -10;
        public PitayaNotFoundException() : base(ErrorCode) { }
        public PitayaNotFoundException(string message) : base(ErrorCode, message) { }

        public override string ToString()
        {
            return "PC_RC_NOT_FOUND";
        }
    }

}