using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// An element segment, which can initialize a table (active) or hold refs for later use (passive/declarative).
/// </summary>
public class Element
{
    /// <summary>
    /// The segment kind (0–7), encoding active/passive/declarative and init-expression vs func-index forms.
    /// </summary>
    public uint Kind { get; set; }

    /// <summary>
    /// The table index. Only meaningful for active segments (kind 0, 2, 4, 6). Kind 0/4 implicitly target table 0.
    /// </summary>
    public uint Index { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IList<Instruction>? initializerExpression;

    /// <summary>
    /// An initializer expression that computes the offset at which to place the elements.
    /// Only meaningful for active segments.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<Instruction> InitializerExpression
    {
        get => this.initializerExpression ??= [];
        set => this.initializerExpression = value ?? throw new ArgumentNullException(nameof(value));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IList<uint>? elements;

    /// <summary>
    /// A sequence of function indices. Used by kinds 0–3 (func-index form).
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<uint> Elements
    {
        get => this.elements ??= [];
        set => this.elements = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The element type for non-func-index forms (kinds 5–7) or explicit-elemtype forms (kinds 1–3).
    /// </summary>
    public ElementType ElemType { get; set; } = ElementType.FunctionReference;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IList<IList<Instruction>>? initExprs;

    /// <summary>
    /// Per-element initializer expressions. Used by kinds 4–7 (init-expression form).
    /// </summary>
    public IList<IList<Instruction>> InitExprs
    {
        get => this.initExprs ??= [];
        set => this.initExprs = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Creates a new <see cref="Element"/> instance.</summary>
    public Element()
    {
    }

    /// <summary>Creates a new <see cref="Element"/> instance with the provided elements (kind 0).</summary>
    public Element(uint offset, params uint[] elements)
        : this(offset, (IList<uint>)elements)
    {
    }

    /// <summary>Creates a new <see cref="Element"/> instance with the provided elements (kind 0).</summary>
    public Element(uint offset, IList<uint> elements)
    {
        this.initializerExpression =
        [
            new Int32Constant(offset),
            new End(),
        ];
        this.elements = elements;
    }

    internal Element(Reader reader)
    {
        Kind = reader.ReadVarUInt32();

        switch (Kind)
        {
            case 0:
                // Active, table 0, i32 offset expr, func indices
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                ReadFuncIndices(reader);
                break;

            case 1:
                // Passive, func indices, prefixed with 0x00 elemkind byte
                _ = reader.ReadByte(); // elemkind = 0x00 (funcref)
                ReadFuncIndices(reader);
                break;

            case 2:
                // Active, explicit table index, i32 offset expr, func indices, prefixed with 0x00 elemkind
                Index = reader.ReadVarUInt32();
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                _ = reader.ReadByte(); // elemkind = 0x00 (funcref)
                ReadFuncIndices(reader);
                break;

            case 3:
                // Declarative, func indices, prefixed with 0x00 elemkind
                _ = reader.ReadByte(); // elemkind = 0x00 (funcref)
                ReadFuncIndices(reader);
                break;

            case 4:
                // Active, table 0, i32 offset expr, init expressions
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                ReadInitExprs(reader);
                break;

            case 5:
                // Passive, init expressions, prefixed with reftype
                ElemType = (ElementType)reader.ReadVarInt7();
                ReadInitExprs(reader);
                break;

            case 6:
                // Active, explicit table index, i32 offset expr, init expressions, prefixed with reftype
                Index = reader.ReadVarUInt32();
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                ElemType = (ElementType)reader.ReadVarInt7();
                ReadInitExprs(reader);
                break;

            case 7:
                // Declarative, init expressions, prefixed with reftype
                ElemType = (ElementType)reader.ReadVarInt7();
                ReadInitExprs(reader);
                break;

            default:
                throw new ModuleLoadException($"Unsupported element segment kind {Kind}.", reader.Offset);
        }
    }

    private void ReadFuncIndices(Reader reader)
    {
        var count = checked((int)reader.ReadVarUInt32());
        var list = this.elements = new List<uint>(count);
        for (var i = 0; i < count; i++)
            list.Add(reader.ReadVarUInt32());
    }

    private void ReadInitExprs(Reader reader)
    {
        var count = checked((int)reader.ReadVarUInt32());
        var list = this.initExprs = new List<IList<Instruction>>(count);
        for (var i = 0; i < count; i++)
            list.Add(Instruction.ParseInitializerExpression(reader).ToList());
    }

    /// <summary>Expresses the value of this instance as a string.</summary>
    public override string ToString() => $"Kind={Kind}, Index={Index}: {InitializerExpression.Count} ({Elements.Count})";

    internal void WriteTo(Writer writer)
    {
        writer.WriteVar(Kind);

        switch (Kind)
        {
            case 0:
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                WriteFuncIndices(writer);
                break;

            case 1:
                writer.Write((byte)0x00);
                WriteFuncIndices(writer);
                break;

            case 2:
                writer.WriteVar(Index);
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                writer.Write((byte)0x00);
                WriteFuncIndices(writer);
                break;

            case 3:
                writer.Write((byte)0x00);
                WriteFuncIndices(writer);
                break;

            case 4:
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                WriteInitExprs(writer);
                break;

            case 5:
                writer.WriteVar((sbyte)ElemType);
                WriteInitExprs(writer);
                break;

            case 6:
                writer.WriteVar(Index);
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                writer.WriteVar((sbyte)ElemType);
                WriteInitExprs(writer);
                break;

            case 7:
                writer.WriteVar((sbyte)ElemType);
                WriteInitExprs(writer);
                break;

            default:
                throw new InvalidOperationException($"Unsupported element segment kind {Kind}.");
        }
    }

    private void WriteFuncIndices(Writer writer)
    {
        writer.WriteVar((uint)this.Elements.Count);
        foreach (var e in this.Elements)
            writer.WriteVar(e);
    }

    private void WriteInitExprs(Writer writer)
    {
        writer.WriteVar((uint)this.InitExprs.Count);
        foreach (var expr in this.InitExprs)
            foreach (var instruction in expr)
                instruction.WriteTo(writer);
    }
}
