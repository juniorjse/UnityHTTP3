using System;

namespace Wildlife.PitayaCSharp.Serializer
{
    public interface IPitayaSerializer
    {
        byte[] Encode(object obj);
        T Decode<T>(byte[] buffer);
    }
}