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
    /// The segment kind, encoding active/passive/declarative and init-expression vs function-index forms.
    /// </summary>
    public ElementKind Kind { get; set; }

    /// <summary>
    /// True for active segments (those that initialize a table at an offset). Passive and declarative segments are not.
    /// </summary>
    public bool IsActive => ((uint)this.Kind & 1) == 0;

    /// <summary>
    /// True for declarative segments.
    /// </summary>
    public bool IsDeclarative =>
        this.Kind is ElementKind.DeclarativeFunctionIndices or ElementKind.DeclarativeExpressions;

    /// <summary>
    /// True when the segment carries per-element initializer expressions (<see cref="InitExprs"/>) rather than plain
    /// function indices (<see cref="Elements"/>).
    /// </summary>
    public bool UsesExpressions => ((uint)this.Kind & 4) != 0;

    /// <summary>
    /// The table index. Only meaningful for active segments with an explicit table index
    /// (<see cref="ElementKind.ActiveExplicitTableFunctionIndices"/>, <see cref="ElementKind.ActiveExplicitTableExpressions"/>);
    /// other active segments implicitly target table 0.
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
    /// A sequence of function indices. Used by the function-index forms (where !UsesExpressions).
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<uint> Elements
    {
        get => this.elements ??= [];
        set => this.elements = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The element type for the reference-typed forms (where UsesExpressions).
    /// </summary>
    public ElementType ElemType { get; set; } = ElementType.FunctionReference;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<IList<Instruction>>? initExprs;

    /// <summary>
    /// Per-element initializer expressions. Used by the init-expression forms (where UsesExpressions).
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
    /// Creates a new active <see cref="Element"/> instance with the provided elements.
    /// </summary>
    /// <param name="offset">The zero-based offset from the start of the table where <paramref name="elements"/> are placed, retained as the <see cref="InitializerExpression"/>.</param>
    /// <param name="elements">The table entries.</param>
    public Element(uint offset, params uint[] elements)
        : this(offset, (IList<uint>)elements)
    {
    }

    /// <summary>
    /// Creates a new active <see cref="Element"/> instance with the provided elements.
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
        this.Kind = (ElementKind)reader.ReadVarUInt32();

        switch (this.Kind)
        {
            case ElementKind.ActiveFunctionIndices:
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.ReadFuncIndices(reader);
                break;

            case ElementKind.PassiveFunctionIndices:
                _ = reader.ReadByte(); //elemkind = 0x00 (funcref)
                this.ReadFuncIndices(reader);
                break;

            case ElementKind.ActiveExplicitTableFunctionIndices:
                this.Index = reader.ReadVarUInt32();
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                _ = reader.ReadByte(); //elemkind = 0x00 (funcref)
                this.ReadFuncIndices(reader);
                break;

            case ElementKind.DeclarativeFunctionIndices:
                _ = reader.ReadByte(); //elemkind = 0x00 (funcref)
                this.ReadFuncIndices(reader);
                break;

            case ElementKind.ActiveExpressions:
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.ReadInitExprs(reader);
                break;

            case ElementKind.PassiveExpressions:
                this.ElemType = (ElementType)reader.ReadVarInt7();
                this.ReadInitExprs(reader);
                break;

            case ElementKind.ActiveExplicitTableExpressions:
                this.Index = reader.ReadVarUInt32();
                this.initializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
                this.ElemType = (ElementType)reader.ReadVarInt7();
                this.ReadInitExprs(reader);
                break;

            case ElementKind.DeclarativeExpressions:
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
        writer.WriteVar((uint)this.Kind);

        switch (this.Kind)
        {
            case ElementKind.ActiveFunctionIndices:
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                this.WriteFuncIndices(writer);
                break;

            case ElementKind.PassiveFunctionIndices:
                writer.Write((byte)0x00);
                this.WriteFuncIndices(writer);
                break;

            case ElementKind.ActiveExplicitTableFunctionIndices:
                writer.WriteVar(this.Index);
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                writer.Write((byte)0x00);
                this.WriteFuncIndices(writer);
                break;

            case ElementKind.DeclarativeFunctionIndices:
                writer.Write((byte)0x00);
                this.WriteFuncIndices(writer);
                break;

            case ElementKind.ActiveExpressions:
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                this.WriteInitExprs(writer);
                break;

            case ElementKind.PassiveExpressions:
                writer.WriteVar((sbyte)this.ElemType);
                this.WriteInitExprs(writer);
                break;

            case ElementKind.ActiveExplicitTableExpressions:
                writer.WriteVar(this.Index);
                foreach (var instruction in this.InitializerExpression)
                    instruction.WriteTo(writer);
                writer.WriteVar((sbyte)this.ElemType);
                this.WriteInitExprs(writer);
                break;

            case ElementKind.DeclarativeExpressions:
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
