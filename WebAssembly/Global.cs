using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WebAssembly;

/// <summary>
/// Describes a global variable.
/// </summary>
public class Global
{
    private WebAssemblyValueType contentType;
    private ReferenceType? contentRefType;

    /// <summary>
    /// Type of the value. For a WASM 3.0 long-form typed reference this reports
    /// <see cref="WebAssemblyValueType.Reference"/>/<see cref="WebAssemblyValueType.NullableReference"/>;
    /// the heap-type detail is available via <see cref="ContentRefType"/>. Setting this clears <see cref="ContentRefType"/>.
    /// </summary>
    public WebAssemblyValueType ContentType
    {
        get => this.contentRefType?.ToValueType() ?? this.contentType;
        set { this.contentType = value; this.contentRefType = null; }
    }

    /// <summary>
    /// The full WASM 3.0 reference type when the content type is a long-form typed reference, otherwise null.
    /// </summary>
    public ReferenceType? ContentRefType
    {
        get => this.contentRefType;
        set => this.contentRefType = value;
    }

    /// <summary>
    /// When true, the value can be changed by running code.
    /// </summary>
    public bool IsMutable { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<Instruction>? initializerExpression;

    /// <summary>
    /// The initial value of the global.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<Instruction> InitializerExpression
    {
        get => this.initializerExpression ??= [];
        set => this.initializerExpression = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new <see cref="Global"/> instance.
    /// </summary>
    public Global()
    {
    }


    /// <summary>
    /// Creates a new <see cref="Global"/> instance with the provided content type.
    /// </summary>
    /// <param name="contentType">The <see cref="ContentType"/> value.</param>
    public Global(WebAssemblyValueType contentType)
    {
        this.ContentType = contentType;
    }

    /// <summary>
    /// Creates a new <see cref="Global"/> instance with the provided reference content type.
    /// </summary>
    /// <param name="contentRefType">The <see cref="ContentRefType"/> value.</param>
    public Global(ReferenceType contentRefType)
    {
        this.ContentRefType = contentRefType;
    }

    /// <summary>
    /// Creates a new <see cref="Global"/> instance with the provided content type and initializer expression.
    /// </summary>
    /// <param name="contentType">The <see cref="ContentType"/> value.</param>
    /// <param name="initializerExpression">The <see cref="InitializerExpression"/> value.</param>
    public Global(WebAssemblyValueType contentType, params Instruction[] initializerExpression)
    {
        this.ContentType = contentType;
        this.initializerExpression = initializerExpression;
    }

    internal Global(Reader reader)
    {
        this.contentType = ReferenceType.ReadValueType(reader, out this.contentRefType);
        this.IsMutable = reader.ReadVarUInt1() == 1;
        this.InitializerExpression = Instruction.ParseInitializerExpression(reader).ToList();
    }

    internal void WriteTo(Writer writer)
    {
        if (this.contentRefType is { } rt)
            rt.WriteTo(writer);
        else
            writer.WriteVar((sbyte)this.contentType);
        writer.WriteVar(this.IsMutable ? 1u : 0);
        foreach (var instruction in this.InitializerExpression)
            instruction.WriteTo(writer);
    }
}
