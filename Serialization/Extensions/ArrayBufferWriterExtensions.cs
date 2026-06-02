using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace AzotBase.Common.Serialization.Extensions;

public static class ArrayBufferWriterExtensions
{
    public static void WriteBool(this ArrayBufferWriter<byte> w, bool value)
    {
        var span = w.GetSpan(1);
        span[0] = value ? (byte)1 : (byte)0;
        w.Advance(1);
    }

    public static void WriteByte(this ArrayBufferWriter<byte> w, byte value)
    {
        var span = w.GetSpan(1);
        span[0] = value;
        w.Advance(1);
    }

    public static void WriteSByte(this ArrayBufferWriter<byte> w, sbyte value)
    {
        var span = w.GetSpan(1);
        span[0] = (byte)value;
        w.Advance(1);
    }

    public static void WriteInt16(this ArrayBufferWriter<byte> w, short value)
    {
        var span = w.GetSpan(2);
        BinaryPrimitives.WriteInt16LittleEndian(span, value);
        w.Advance(2);
    }

    public static void WriteUInt16(this ArrayBufferWriter<byte> w, ushort value)
    {
        var span = w.GetSpan(2);
        BinaryPrimitives.WriteUInt16LittleEndian(span, value);
        w.Advance(2);
    }

    public static void WriteInt32(this ArrayBufferWriter<byte> w, int value)
    {
        var span = w.GetSpan(4);
        BinaryPrimitives.WriteInt32LittleEndian(span, value);
        w.Advance(4);
    }

    public static void WriteUInt32(this ArrayBufferWriter<byte> w, uint value)
    {
        var span = w.GetSpan(4);
        BinaryPrimitives.WriteUInt32LittleEndian(span, value);
        w.Advance(4);
    }

    public static void WriteInt64(this ArrayBufferWriter<byte> w, long value)
    {
        var span = w.GetSpan(8);
        BinaryPrimitives.WriteInt64LittleEndian(span, value);
        w.Advance(8);
    }

    public static void WriteUInt64(this ArrayBufferWriter<byte> w, ulong value)
    {
        var span = w.GetSpan(8);
        BinaryPrimitives.WriteUInt64LittleEndian(span, value);
        w.Advance(8);
    }

    public static void WriteSingle(this ArrayBufferWriter<byte> w, float value)
    {
        var span = w.GetSpan(4);
        BinaryPrimitives.WriteSingleLittleEndian(span, value);
        w.Advance(4);
    }

    public static void WriteDouble(this ArrayBufferWriter<byte> w, double value)
    {
        var span = w.GetSpan(8);
        BinaryPrimitives.WriteDoubleLittleEndian(span, value);
        w.Advance(8);
    }

    public static void WriteDecimal(this ArrayBufferWriter<byte> w, decimal value)
    {
        var bits = decimal.GetBits(value);
        var span = w.GetSpan(16);
        BinaryPrimitives.WriteInt32LittleEndian(span[0..], bits[0]);
        BinaryPrimitives.WriteInt32LittleEndian(span[4..], bits[1]);
        BinaryPrimitives.WriteInt32LittleEndian(span[8..], bits[2]);
        BinaryPrimitives.WriteInt32LittleEndian(span[12..], bits[3]);
        w.Advance(16);
    }

    public static void WriteString(this ArrayBufferWriter<byte> w, string value)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        w.WriteInt32(byteCount);
        var span = w.GetSpan(byteCount);
        Encoding.UTF8.GetBytes(value, span);
        w.Advance(byteCount);
    }
}