using System;

namespace Wildlife.PitayaCSharp.Transport
{
    public class TransporterSendItem
    {
        public const byte TypeMask = 0xf0;

        public TransporterSendItemType Type { get; private set; }
        public readonly byte[] Buffer;
        public readonly uint SequenceNumber; // For notify, if internal use -1
        public readonly uint RequestUid; // For request, if internal use -1
        public readonly DateTime Timestamp;
        public readonly int Timeout;

        public TransporterSendItem(byte[] buffer, byte allocationType, uint sequenceNumber = uint.MaxValue, uint requestUid = uint.MaxValue, int timeout = -1)
        {
            Buffer = buffer;
            SequenceNumber = sequenceNumber;
            RequestUid = requestUid;
            Timestamp = DateTime.Now;
            Timeout = timeout;

            if (sequenceNumber == uint.MaxValue && requestUid == uint.MaxValue)
                SetInternalType(allocationType);
            else if (requestUid == 0)
                SetNotifyType(allocationType);
            else
                SetResponseType(allocationType);
        }

        private void SetInternalType(byte allocationType) { UpdateType(allocationType, TransporterSendItemType.Internal); }
        private void SetNotifyType(byte allocationType) { UpdateType(allocationType, TransporterSendItemType.Notify); }
        private void SetResponseType(byte allocationType) { UpdateType(allocationType, TransporterSendItemType.Response); }

        private void UpdateType(byte allocationType, TransporterSendItemType newType)
        {
            allocationType = (byte)(allocationType & ~TypeMask);
            allocationType |= (byte)newType;

            Type = (TransporterSendItemType)allocationType;
        }
    }
}
