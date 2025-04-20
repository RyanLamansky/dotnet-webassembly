﻿using System;
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
