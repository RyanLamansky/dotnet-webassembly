using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace WebAssembly;

/// <summary>
/// The data section declares the initialized data that is loaded into the linear memory.
/// </summary>
public class Data
{
    /// <summary>
    /// The segment kind: 0 = active (memory 0, offset expression), 1 = passive, 2 = active (explicit memory index + offset expression).
    /// </summary>
    public uint Kind { get; set; }

    /// <summary>
    /// The linear memory index. Only meaningful for <see cref="Kind"/> 2; <see cref="Kind"/> 0 implicitly targets memory 0.
    /// </summary>
    public uint MemoryIndex { get; set; }

    /// <summary>
    /// Obsolete alias for <see cref="MemoryIndex"/>, retained for source compatibility with code written before
    /// WebAssembly 2.0 data segment kinds were introduced (when this was simply the always-zero memory index).
    /// </summary>
    [Obsolete("Use MemoryIndex for the target memory index, and Kind for the segment kind (active/passive).")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public uint Index
    {
        get => this.MemoryIndex;
        set => this.MemoryIndex = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<Instruction>? initializerExpression;

    /// <summary>
    /// An <see cref="WebAssemblyValueType.Int32"/> initializer expression that computes the offset at which to place the data.
    /// Only meaningful for active segments (<see cref="Kind"/> 0 and 2).
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<Instruction> InitializerExpression
    {
        get => this.initializerExpression ??= [];
        set => this.initializerExpression = value ?? throw new ArgumentNullException(nameof(value));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
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

    /// <summary>
    /// Creates a new <see cref="Data"/> instance.
    /// </summary>
    public Data()
    {
    }

    internal Data(Reader reader)
    {
        this.Kind = reader.ReadVarUInt32();

        switch (this.Kind)
        {
            case 0: //Active, memory 0, offset initializer, bytes.
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.rawData = reader.ReadBytes(reader.ReadVarUInt32());
                break;

            case 1: //Passive — no memory, no offset; just bytes.
                this.rawData = reader.ReadBytes(reader.ReadVarUInt32());
                break;

            case 2: //Active, explicit memory index, offset initializer, bytes.
                this.MemoryIndex = reader.ReadVarUInt32();
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.rawData = reader.ReadBytes(reader.ReadVarUInt32());
                break;

            default:
                throw new ModuleLoadException($"Unsupported data segment kind {this.Kind}.", reader.Offset);
        }
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"Kind: {Kind}, Length: {rawData?.Count}";

    internal void WriteTo(Writer writer)
    {
        writer.WriteVar(this.Kind);

        switch (this.Kind)
        {
            case 0:
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                this.WriteRawData(writer);
                break;

            case 1:
                this.WriteRawData(writer);
                break;

            case 2:
                writer.WriteVar(this.MemoryIndex);
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                this.WriteRawData(writer);
                break;

            default:
                throw new InvalidOperationException($"Unsupported data segment kind {this.Kind}.");
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
