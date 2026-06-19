using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// An element segment, which can initialize a table (active) or hold references for later use (passive/declarative).
/// The elements section allows a module to initialize the elements of any imported or internally-defined table.
/// </summary>
public class Element
{
    /// <summary>
    /// The segment kind (0–7), encoding active/passive/declarative and init-expression vs function-index forms.
    /// </summary>
    public uint Kind { get; set; }

    /// <summary>
    /// The table index. Only meaningful for active segments (kind 0, 2, 4, 6); kind 0/4 implicitly target table 0.
    /// </summary>
    public uint Index { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
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

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<uint>? elements;

    /// <summary>
    /// A sequence of function indices. Used by the function-index forms (kinds 0–3).
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<uint> Elements
    {
        get => this.elements ??= [];
        set => this.elements = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The element type for the reference-typed forms (kinds 5–7).
    /// </summary>
    public ElementType ElemType { get; set; } = ElementType.FunctionReference;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<IList<Instruction>>? initExprs;

    /// <summary>
    /// Per-element initializer expressions. Used by the init-expression forms (kinds 4–7).
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<IList<Instruction>> InitExprs
    {
        get => this.initExprs ??= [];
        set => this.initExprs = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new <see cref="Element"/> instance.
    /// </summary>
    public Element()
    {
    }

    /// <summary>
    /// Creates a new active (kind 0) <see cref="Element"/> instance with the provided elements.
    /// </summary>
    /// <param name="offset">The zero-based offset from the start of the table where <paramref name="elements"/> are placed, retained as the <see cref="InitializerExpression"/>.</param>
    /// <param name="elements">The table entries.</param>
    public Element(uint offset, params uint[] elements)
        : this(offset, (IList<uint>)elements)
    {
    }

    /// <summary>
    /// Creates a new active (kind 0) <see cref="Element"/> instance with the provided elements.
    /// </summary>
    /// <param name="offset">The zero-based offset from the start of the table where <paramref name="elements"/> are placed, retained as the <see cref="InitializerExpression"/>.</param>
    /// <param name="elements">The table entries.</param>
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
        this.Kind = reader.ReadVarUInt32();

        switch (this.Kind)
        {
            case 0: //Active, table 0, i32 offset expression, function indices.
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.ReadFuncIndices(reader);
                break;

            case 1: //Passive, function indices, prefixed with a 0x00 elemkind byte.
                _ = reader.ReadByte(); //elemkind = 0x00 (funcref)
                this.ReadFuncIndices(reader);
                break;

            case 2: //Active, explicit table index, i32 offset expression, function indices, prefixed with 0x00 elemkind.
                this.Index = reader.ReadVarUInt32();
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                _ = reader.ReadByte(); //elemkind = 0x00 (funcref)
                this.ReadFuncIndices(reader);
                break;

            case 3: //Declarative, function indices, prefixed with 0x00 elemkind.
                _ = reader.ReadByte(); //elemkind = 0x00 (funcref)
                this.ReadFuncIndices(reader);
                break;

            case 4: //Active, table 0, i32 offset expression, init expressions.
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.ReadInitExprs(reader);
                break;

            case 5: //Passive, init expressions, prefixed with reftype.
                this.ElemType = (ElementType)reader.ReadVarInt7();
                this.ReadInitExprs(reader);
                break;

            case 6: //Active, explicit table index, i32 offset expression, init expressions, prefixed with reftype.
                this.Index = reader.ReadVarUInt32();
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.ElemType = (ElementType)reader.ReadVarInt7();
                this.ReadInitExprs(reader);
                break;

            case 7: //Declarative, init expressions, prefixed with reftype.
                this.ElemType = (ElementType)reader.ReadVarInt7();
                this.ReadInitExprs(reader);
                break;

            default:
                throw new ModuleLoadException($"Unsupported element segment kind {this.Kind}.", reader.Offset);
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

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"Kind: {Kind}, Index: {Index}: {InitializerExpression.Count} ({Elements.Count})";

    internal void WriteTo(Writer writer)
    {
        writer.WriteVar(this.Kind);

        switch (this.Kind)
        {
            case 0:
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                this.WriteFuncIndices(writer);
                break;

            case 1:
                writer.Write((byte)0x00);
                this.WriteFuncIndices(writer);
                break;

            case 2:
                writer.WriteVar(this.Index);
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                writer.Write((byte)0x00);
                this.WriteFuncIndices(writer);
                break;

            case 3:
                writer.Write((byte)0x00);
                this.WriteFuncIndices(writer);
                break;

            case 4:
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                this.WriteInitExprs(writer);
                break;

            case 5:
                writer.WriteVar((sbyte)this.ElemType);
                this.WriteInitExprs(writer);
                break;

            case 6:
                writer.WriteVar(this.Index);
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                writer.WriteVar((sbyte)this.ElemType);
                this.WriteInitExprs(writer);
                break;

            case 7:
                writer.WriteVar((sbyte)this.ElemType);
                this.WriteInitExprs(writer);
                break;

            default:
                throw new InvalidOperationException($"Unsupported element segment kind {this.Kind}.");
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
