using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WebAssembly;

/// <summary>
/// The data section declares the initialized data that is loaded into the linear memory.
/// </summary>
public class Data
{
    /// <summary>
    /// The segment kind: 0 = active (memory 0, offset expr), 1 = passive, 2 = active (explicit memory index + offset expr).
    /// </summary>
    public uint Kind { get; set; }

    /// <summary>
    /// The linear memory index. Only meaningful for kind 2. Kind 0 implicitly targets memory 0.
    /// </summary>
    public uint MemoryIndex { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IList<Instruction>? initializerExpression;

    /// <summary>
    /// An <see cref="WebAssemblyValueType.Int32"/> initializer expression that computes the offset at which to place the data.
    /// Only meaningful for active segments (kind 0 and 2).
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<Instruction> InitializerExpression
    {
        get => this.initializerExpression ??= [];
        set => this.initializerExpression = value ?? throw new ArgumentNullException(nameof(value));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IList<byte>? rawData;

    /// <summary>
    /// Raw data in byte form.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<byte> RawData
    {
        get => this.rawData ??= [];
        set => this.rawData = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Creates a new <see cref="Data"/> instance.</summary>
    public Data()
    {
    }

    internal Data(Reader reader)
    {
        Kind = reader.ReadVarUInt32();

        switch (Kind)
        {
            case 0:
                // Active, memory 0, offset initializer, bytes
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.rawData = reader.ReadBytes(reader.ReadVarUInt32());
                break;

            case 1:
                // Passive — no memory, no offset; just bytes
                this.rawData = reader.ReadBytes(reader.ReadVarUInt32());
                break;

            case 2:
                // Active, explicit memory index, offset initializer, bytes
                MemoryIndex = reader.ReadVarUInt32();
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.rawData = reader.ReadBytes(reader.ReadVarUInt32());
                break;

            default:
                throw new ModuleLoadException($"Unsupported data segment kind {Kind}.", reader.Offset);
        }
    }

    /// <summary>Expresses the value of this instance as a string.</summary>
    public override string ToString() => $"Kind={Kind}, Length: {rawData?.Count}";

    internal void WriteTo(Writer writer)
    {
        writer.WriteVar(Kind);

        switch (Kind)
        {
            case 0:
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                WriteRawData(writer);
                break;

            case 1:
                WriteRawData(writer);
                break;

            case 2:
                writer.WriteVar(MemoryIndex);
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                WriteRawData(writer);
                break;

            default:
                throw new InvalidOperationException($"Unsupported data segment kind {Kind}.");
        }
    }

    private void WriteRawData(Writer writer)
    {
        writer.WriteVar((uint)this.RawData.Count);
        if (this.RawData is byte[] bytes)
        {
            writer.Write(bytes);
        }
        else
        {
            foreach (var b in this.RawData)
                writer.Write(b);
        }
    }
}
