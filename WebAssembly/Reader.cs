using System;
using System.IO;
using System.Text;

namespace WebAssembly;

internal sealed class Reader(Stream input) : IDisposable
{
    private BinaryReader? reader = new(input, Encoding.UTF8, true);

    public long Offset { get; private set; }

    public BinaryReader CheckedReader => this.reader ?? throw new ObjectDisposedException(nameof(Reader));

    public uint ReadUInt32()
    {
        var result = CheckedReader.ReadUInt32();
        this.Offset += 4;
        return result;
    }

    public byte ReadVarUInt1() => (byte)(this.ReadVarUInt32() & 0b1);

    public bool TryReadVarUInt7(out byte result)
    {
        try
        {
            result = this.ReadVarUInt7();
            return true;
        }
        catch (EndOfStreamException)
        {
            result = 0;
            return false;
        }
    }

    public byte ReadVarUInt7() => (byte)(this.ReadVarUInt32() & 0b1111111);

    public sbyte ReadVarInt7() => (sbyte)(this.ReadVarInt32() & 0b11111111);

    public uint ReadVarUInt32()
    {
        var result = 0u;
        var shift = 0;
        while (true)
        {
            uint value = this.ReadByte();
            result |= (value & 0x7F) << shift;
            if ((value & 0x80) == 0)
                break;
            shift += 7;
        }

        return result;
    }

    public ulong ReadVarUInt64()
    {
        var result = 0ul;
        var shift = 0;
        var initialOffset = this.Offset;
        while (true)
        {
            ulong value = this.ReadByte();
            // A 64-bit LEB128 spans at most 10 bytes; the 10th byte contributes only its lowest bit (shift 63).
            // Beyond that the value cannot fit in 64 bits, so reject rather than silently wrapping.
            if (shift >= 64 || (shift == 63 && (value & 0x7E) != 0))
                throw new ModuleLoadException("Invalid LEB128 sequence: value exceeds 64 bits.", initialOffset);
            result |= (value & 0x7F) << shift;
            if ((value & 0x80) == 0)
                break;
            shift += 7;
        }

        return result;
    }

    public int ReadVarInt32()
    {
        var result = 0;
        int current;
        var count = 0;
        var signBits = -1;
        var initialOffset = this.Offset;
        do
        {
            current = this.ReadByte();
            result |= (current & 0x7F) << (count * 7);
            signBits <<= 7;
            count++;
        } while (((current & 0x80) == 0x80) && count < 5);

        if ((current & 0x80) == 0x80)
            throw new ModuleLoadException("Invalid LEB128 sequence.", initialOffset);

        if (((signBits >> 1) & result) != 0)
            result |= signBits;

        return result;
    }

    public long ReadVarInt64()
    {
        var result = 0L;
        long current;
        var count = 0;
        var signBits = -1L;
        var initialOffset = this.Offset;
        do
        {
            current = this.ReadByte();
            result |= (current & 0x7F) << (count * 7);
            signBits <<= 7;
            count++;
        } while (((current & 0x80) == 0x80) && count < 10);

        if ((current & 0x80) == 0x80)
            throw new ModuleLoadException("Invalid LEB128 sequence.", initialOffset);

        if (((signBits >> 1) & result) != 0)
            result |= signBits;

        return result;
    }

    /// <summary>
    /// Reads a heap type encoded as an s33 (following a <c>0x63</c>/<c>0x64</c> reference-type lead byte, or as the
    /// immediate of <c>ref.null</c>). A non-negative value is a concrete type index; a negative value is an abstract
    /// heap type. Returns the raw signed value; the caller distinguishes and range-checks it.
    /// </summary>
    public long ReadHeapTypeS33()
    {
        var initialOffset = this.Offset;
        var value = this.ReadVarInt64();
        if (value > uint.MaxValue)
            throw new ModuleLoadException($"Heap type index {value} exceeds the 32-bit range.", initialOffset);
        return value;
    }

    public float ReadFloat32()
    {
        var result = CheckedReader.ReadSingle();
        this.Offset += 4;
        return result;
    }

    public double ReadFloat64()
    {
        var result = CheckedReader.ReadDouble();
        this.Offset += 8;
        return result;
    }

    public string ReadString(uint length)
    {
        var bytes = this.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }

    public byte[] ReadBytes(uint length)
    {
        var result = CheckedReader.ReadBytes(checked((int)length));
        this.Offset += length;
        return result;
    }

    public byte ReadByte()
    {
        var result = CheckedReader.ReadByte();
        this.Offset++;
        return result;
    }

    #region IDisposable Support
    void Dispose(bool disposing)
    {
        if (this.reader == null || !disposing)
            return;

        try //Tolerate bad dispose implementations.
        {
            this.reader.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }

        this.reader = null;
    }

    ~Reader() => Dispose(false);

    /// <summary>
    /// Releases unmanaged resources associated with this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
