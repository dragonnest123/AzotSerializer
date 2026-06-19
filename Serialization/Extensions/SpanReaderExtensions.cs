using System.Buffers.Binary;
using System.Text;

namespace Serialization.Extensions;

public static class SpanReaderExtensions
{
    extension(ref ReadOnlySpan<byte> buffer)
    {
        public bool ReadBool()
        {
            var value = buffer[0] != 0;
            buffer = buffer[1..];
            return value;
        }

        public byte ReadByte()
        {
            var value = buffer[0];
            buffer = buffer[1..];
            return value;
        }
        
        public ReadOnlySpan<byte> ReadBytes(int count)
        {
            var value = buffer[..count];
            buffer = buffer[count..];
            return value;
        }

        public sbyte ReadSByte()
        {
            var value = (sbyte)buffer[0];
            buffer = buffer[1..];
            return value;
        }

        public char ReadChar() => (char)buffer.ReadUInt16();

        public short ReadInt16()
        {
            var value = BinaryPrimitives.ReadInt16LittleEndian(buffer);
            buffer = buffer[2..];
            return value;
        }

        public ushort ReadUInt16()
        {
            var value = BinaryPrimitives.ReadUInt16LittleEndian(buffer);
            buffer = buffer[2..];
            return value;
        }

        public int ReadInt32()
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            buffer = buffer[4..];
            return value;
        }

        public uint ReadUInt32()
        {
            var value = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            buffer = buffer[4..];
            return value;
        }

        public long ReadInt64()
        {
            var value = BinaryPrimitives.ReadInt64LittleEndian(buffer);
            buffer = buffer[8..];
            return value;
        }

        public ulong ReadUInt64()
        {
            var value = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            buffer = buffer[8..];
            return value;
        }

        public float ReadSingle()
        {
            var value = BinaryPrimitives.ReadSingleLittleEndian(buffer);
            buffer = buffer[4..];
            return value;
        }

        public double ReadDouble()
        {
            var value = BinaryPrimitives.ReadDoubleLittleEndian(buffer);
            buffer = buffer[8..];
            return value;
        }

        public decimal ReadDecimal()
        {
            var bits = new int[4];
            bits[0] = BinaryPrimitives.ReadInt32LittleEndian(buffer[0..]);
            bits[1] = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);
            bits[2] = BinaryPrimitives.ReadInt32LittleEndian(buffer[8..]);
            bits[3] = BinaryPrimitives.ReadInt32LittleEndian(buffer[12..]);
            buffer = buffer[16..];
            return new decimal(bits);
        }

        public string ReadString()
        {
            var length = buffer.ReadInt32();
            var value = Encoding.UTF8.GetString(buffer[..length]);
            buffer = buffer[length..];
            return value;
        }
        
        public DateTime ReadDateTime()
        {
            var ticks = buffer.ReadInt64();
            var kind = (DateTimeKind)buffer.ReadByte();
            return new DateTime(ticks, kind);
        }

        public TimeSpan ReadTimeSpan()
        {
            var ticks = buffer.ReadInt64();
            return new TimeSpan(ticks);
        }
    }
}