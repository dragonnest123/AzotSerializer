using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Serialization.Extensions;

public static class BufferWriterExtensions
{
    extension(IBufferWriter<byte> w)
    {
        public void WriteBool(bool value)
        {
            var span = w.GetSpan(1);
            span[0] = value ? (byte)1 : (byte)0;
            w.Advance(1);
        }

        public void WriteByte(byte value)
        {
            var span = w.GetSpan(1);
            span[0] = value;
            w.Advance(1);
        }

        public void WriteSByte(sbyte value)
        {
            var span = w.GetSpan(1);
            span[0] = (byte)value;
            w.Advance(1);
        }
        
        public void WriteChar(char value) => w.WriteUInt16(value);

        public void WriteInt16(short value)
        {
            var span = w.GetSpan(2);
            BinaryPrimitives.WriteInt16LittleEndian(span, value);
            w.Advance(2);
        }

        public void WriteUInt16(ushort value)
        {
            var span = w.GetSpan(2);
            BinaryPrimitives.WriteUInt16LittleEndian(span, value);
            w.Advance(2);
        }

        public void WriteInt32(int value)
        {
            var span = w.GetSpan(4);
            BinaryPrimitives.WriteInt32LittleEndian(span, value);
            w.Advance(4);
        }

        public void WriteUInt32(uint value)
        {
            var span = w.GetSpan(4);
            BinaryPrimitives.WriteUInt32LittleEndian(span, value);
            w.Advance(4);
        }

        public void WriteInt64(long value)
        {
            var span = w.GetSpan(8);
            BinaryPrimitives.WriteInt64LittleEndian(span, value);
            w.Advance(8);
        }

        public void WriteUInt64(ulong value)
        {
            var span = w.GetSpan(8);
            BinaryPrimitives.WriteUInt64LittleEndian(span, value);
            w.Advance(8);
        }

        public void WriteSingle(float value)
        {
            var span = w.GetSpan(4);
            BinaryPrimitives.WriteSingleLittleEndian(span, value);
            w.Advance(4);
        }

        public void WriteDouble(double value)
        {
            var span = w.GetSpan(8);
            BinaryPrimitives.WriteDoubleLittleEndian(span, value);
            w.Advance(8);
        }

        public void WriteDecimal(decimal value)
        {
            var bits = decimal.GetBits(value);
            var span = w.GetSpan(16);
            BinaryPrimitives.WriteInt32LittleEndian(span[0..], bits[0]);
            BinaryPrimitives.WriteInt32LittleEndian(span[4..], bits[1]);
            BinaryPrimitives.WriteInt32LittleEndian(span[8..], bits[2]);
            BinaryPrimitives.WriteInt32LittleEndian(span[12..], bits[3]);
            w.Advance(16);
        }

        public void WriteString(string value)
        {
            var byteCount = Encoding.UTF8.GetByteCount(value);
            w.WriteInt32(byteCount);
            var span = w.GetSpan(byteCount);
            Encoding.UTF8.GetBytes(value, span);
            w.Advance(byteCount);
        }
        
        public void WriteDateTime(DateTime value)
        {
            w.WriteInt64(value.Ticks);
            w.WriteByte((byte)value.Kind);
        }

        public void WriteTimeSpan(TimeSpan value)
        {
            w.WriteInt64(value.Ticks);
        }
    }
}