using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WebAssembly;

/// <summary>
/// Desribes a table within the assembly.
/// </summary>
public class Table
{
    private ElementType elementType;
    private ReferenceType? elementRefType;

    /// <summary>
    /// The type of elements. For a WASM 3.0 long-form typed reference this reports
    /// <see cref="ElementType.Reference"/>/<see cref="ElementType.NullableReference"/>; the heap-type detail is
    /// available via <see cref="ElementRefType"/>. Setting this clears <see cref="ElementRefType"/>.
    /// </summary>
    public ElementType ElementType
    {
        get => this.elementRefType is { } rt ? (ElementType)(sbyte)rt.ToValueType() : this.elementType;
        set { this.elementType = value; this.elementRefType = null; }
    }

    /// <summary>
    /// The full WASM 3.0 reference type when the element type is a long-form typed reference, otherwise null.
    /// </summary>
    public ReferenceType? ElementRefType
    {
        get => this.elementRefType;
        set => this.elementRefType = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<Instruction>? initializerExpression;

    /// <summary>
    /// The WASM 3.0 table-with-initializer const expression (the <c>0x40 0x00</c> encoding), used to populate every
    /// slot of the table at instantiation. Null for the classic encoding, in which case slots start as null references.
    /// </summary>
    public IList<Instruction>? InitializerExpression
    {
        get => this.initializerExpression;
        set => this.initializerExpression = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private ResizableLimits? resizableLimits;

    /// <summary>
    /// A packed tuple that describes the limits of the table.
    /// </summary>
    public ResizableLimits ResizableLimits
    {
        get => resizableLimits ??= new ResizableLimits();
        set => resizableLimits = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// When true, this is a 64-bit (table64) table whose accesses use i64 indices; when false, a 32-bit table
    /// using i32 indices. This is a convenience view of <see cref="WebAssembly.ResizableLimits.Is64Bit"/>.
    /// </summary>
    public bool Is64Bit
    {
        get => this.ResizableLimits.Is64Bit;
        set => this.ResizableLimits.Is64Bit = value;
    }

    /// <summary>
    /// Creates a new <see cref="Table"/> instance.
    /// </summary>
    public Table()
    {
        this.elementType = ElementType.FunctionReference;
    }

    /// <summary>
    /// Creates a new <see cref="Table"/> instance with the provided <see cref="ResizableLimits.Minimum"/> and <see cref="ResizableLimits.Maximum"/> values.
    /// </summary>
    /// <param name="minimum">Initial length (in units of table elements or 65,536-byte pages).</param>
    /// <param name="maximum">Maximum length (in units of table elements or 65,536-byte pages).</param>
    public Table(uint minimum, uint? maximum = null)
        : this()
    {
        this.resizableLimits = new ResizableLimits(minimum, maximum);
    }

    /// <summary>
    /// Creates a new <see cref="Table"/> from a binary data stream.
    /// </summary>
    /// <param name="reader">The source of data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
    internal Table(Reader reader)
    {
        // WASM 3.0 table-with-initializer: a leading 0x40 (s7 -0x40, impossible as a reftype) then 0x00 selects the
        // form "0x40 0x00 tabletype constexpr". The classic encoding is just "elementtype limits".
        var lead = reader.ReadVarInt7();
        if (lead == -0x40)
        {
            _ = reader.ReadByte(); // 0x00 (reserved)
            this.ReadTableType(reader, reader.ReadVarInt7());
            this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
        }
        else
        {
            this.ReadTableType(reader, lead);
        }
    }

    private void ReadTableType(Reader reader, sbyte lead)
    {
        var valueType = (WebAssemblyValueType)lead;
        if (valueType is WebAssemblyValueType.Reference or WebAssemblyValueType.NullableReference)
            this.elementRefType = ReferenceType.Parse(reader, lead);
        this.elementType = (ElementType)lead;
        this.resizableLimits = new ResizableLimits(reader);
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"Table {ElementType}, {ResizableLimits}";

    internal void WriteTo(Writer writer)
    {
        if (this.initializerExpression is { } init)
        {
            writer.Write(0x40);
            writer.Write(0x00);
            this.WriteTableType(writer);
            foreach (var instruction in init)
                instruction.WriteTo(writer);
        }
        else
        {
            this.WriteTableType(writer);
        }
    }

    private void WriteTableType(Writer writer)
    {
        if (this.elementRefType is { } rt)
            rt.WriteTo(writer);
        else
            writer.WriteVar((sbyte)this.elementType);
        this.ResizableLimits.WriteTo(writer);
    }
}
