using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace WebAssembly;

/// <summary>
/// Describes a WebAssembly composite type: a function signature, or — with WebAssembly 3.0 garbage collection —
/// a struct or array (selected by <see cref="Form"/>).
/// </summary>
public class WebAssemblyType : IEquatable<WebAssemblyType>
{
    /// <summary>
    /// The kind of composite type.  The only accepted value in the initial binary format is <see cref="FunctionType.Function"/>, which is the default.
    /// The WASM 3.0 garbage-collection values <see cref="FunctionType.Struct"/> and <see cref="FunctionType.Array"/> use <see cref="Fields"/>.
    /// </summary>
    public FunctionType Form { get; set; }

    /// <summary>
    /// The fields of a <see cref="FunctionType.Struct"/> composite type, or the single-element list holding the element
    /// type of a <see cref="FunctionType.Array"/> composite type (WASM 3.0 garbage collection).  Null for a
    /// <see cref="FunctionType.Function"/>.
    /// </summary>
    public IList<StructField>? Fields { get; set; }

    /// <summary>
    /// When true (the default, matching the shorthand composite encoding), no further subtypes of this type may be
    /// declared.  A final type may still declare a supertype (see <see cref="SuperTypes"/>).
    /// </summary>
    public bool IsFinal { get; set; } = true;

    /// <summary>
    /// The declared supertypes of this type (WASM 3.0 garbage collection), or null/empty when the type declares none.
    /// Ratified WASM 3.0 permits at most one entry, but the vector is modeled faithfully.
    /// </summary>
    public IList<uint>? SuperTypes { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<WebAssemblyValueType>? parameters;

    /// <summary>
    /// Parameters to the function.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<WebAssemblyValueType> Parameters
    {
        get => this.parameters ??= [];
        set => this.parameters = value ?? throw new ArgumentNullException(nameof(value));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private IList<WebAssemblyValueType>? returns;

    /// <summary>
    /// Return types to the function.  For the initial binary format, a maximum of 1 is allowed.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<WebAssemblyValueType> Returns
    {
        get => this.returns ??= [];
        set => this.returns = value ?? throw new ArgumentNullException(nameof(value));
    }

    // Positional sidecars carrying the full WASM 3.0 reference type for parameter/return slots whose narrow entry is
    // Reference/NullableReference. Keyed by index into Parameters/Returns; positional edits must keep these in sync.
    private Dictionary<int, ReferenceType>? parameterRefTypes;
    private Dictionary<int, ReferenceType>? returnRefTypes;

    /// <summary>
    /// Gets the full WASM 3.0 reference type of the parameter at <paramref name="index"/>, or null if that parameter
    /// is not a long-form typed reference.
    /// </summary>
    /// <param name="index">The index into <see cref="Parameters"/>.</param>
    public ReferenceType? GetParameterRefType(int index) =>
        this.parameterRefTypes is { } map && map.TryGetValue(index, out var refType) ? refType : null;

    /// <summary>
    /// Sets the parameter at <paramref name="index"/> to the given long-form typed reference, updating both
    /// <see cref="Parameters"/> and the sidecar.
    /// </summary>
    /// <param name="index">The index into <see cref="Parameters"/>.</param>
    /// <param name="refType">The reference type.</param>
    public void SetParameterRefType(int index, ReferenceType refType)
    {
        this.Parameters[index] = refType.ToValueType();
        (this.parameterRefTypes ??= [])[index] = refType;
    }

    /// <summary>
    /// Gets the full WASM 3.0 reference type of the return at <paramref name="index"/>, or null if that return is not
    /// a long-form typed reference.
    /// </summary>
    /// <param name="index">The index into <see cref="Returns"/>.</param>
    public ReferenceType? GetReturnRefType(int index) =>
        this.returnRefTypes is { } map && map.TryGetValue(index, out var refType) ? refType : null;

    /// <summary>
    /// Sets the return at <paramref name="index"/> to the given long-form typed reference, updating both
    /// <see cref="Returns"/> and the sidecar.
    /// </summary>
    /// <param name="index">The index into <see cref="Returns"/>.</param>
    /// <param name="refType">The reference type.</param>
    public void SetReturnRefType(int index, ReferenceType refType)
    {
        this.Returns[index] = refType.ToValueType();
        (this.returnRefTypes ??= [])[index] = refType;
    }

    /// <summary>
    /// Appends a long-form typed-reference parameter, keeping <see cref="Parameters"/> and the sidecar in sync.
    /// </summary>
    /// <param name="refType">The reference type.</param>
    public void AddParameter(ReferenceType refType)
    {
        var parameters = this.Parameters;
        parameters.Add(refType.ToValueType());
        (this.parameterRefTypes ??= [])[parameters.Count - 1] = refType;
    }

    /// <summary>
    /// Appends a long-form typed-reference return, keeping <see cref="Returns"/> and the sidecar in sync.
    /// </summary>
    /// <param name="refType">The reference type.</param>
    public void AddReturn(ReferenceType refType)
    {
        var returns = this.Returns;
        returns.Add(refType.ToValueType());
        (this.returnRefTypes ??= [])[returns.Count - 1] = refType;
    }

    /// <summary>
    /// Creates a new <see cref="WebAssemblyType"/> instance.
    /// </summary>
    public WebAssemblyType()
    {
        this.Form = FunctionType.Function;
    }

    // Reads a single composite type whose lead byte has already been consumed as compositeForm; the sub-type wrapper
    // (finality and declared supertypes) is decoded by CompositeTypeReader and supplied here.
    internal WebAssemblyType(Reader reader, sbyte compositeForm, bool isFinal, IList<uint>? superTypes)
    {
        this.Form = (FunctionType)compositeForm;
        this.IsFinal = isFinal;
        this.SuperTypes = superTypes;

        switch (compositeForm)
        {
            case CompositeTypeReader.FunctionForm:
                {
                    var count = checked((int)reader.ReadVarUInt32());
                    var parameters = this.parameters = new List<WebAssemblyValueType>(count);
                    for (var i = 0; i < count; i++)
                    {
                        parameters.Add(ReferenceType.ReadValueType(reader, out var refType));
                        if (refType is { } r)
                            (this.parameterRefTypes ??= [])[i] = r;
                    }

                    count = checked((int)reader.ReadVarUInt32());
                    var returns = this.returns = new List<WebAssemblyValueType>(count);
                    for (var i = 0; i < count; i++)
                    {
                        returns.Add(ReferenceType.ReadValueType(reader, out var refType));
                        if (refType is { } r)
                            (this.returnRefTypes ??= [])[i] = r;
                    }
                }
                break;

            case CompositeTypeReader.StructForm:
                {
                    var count = checked((int)reader.ReadVarUInt32());
                    var fields = new List<StructField>(count);
                    for (var i = 0; i < count; i++)
                        fields.Add(ReadField(reader));
                    this.Fields = fields;
                }
                break;

            case CompositeTypeReader.ArrayForm:
                this.Fields = new List<StructField>(1) { ReadField(reader) };
                break;

            default:
                throw new ModuleLoadException($"Unrecognized composite type form 0x{(byte)compositeForm:X2}.", reader.Offset);
        }
    }

    private static StructField ReadField(Reader reader)
    {
        var lead = reader.ReadVarInt7();
        var field = new StructField();
        switch (lead)
        {
            case CompositeTypeReader.PackedI8Form:
                field.Packed = PackedType.I8;
                break;
            case CompositeTypeReader.PackedI16Form:
                field.Packed = PackedType.I16;
                break;
            default:
                if (lead is (sbyte)WebAssemblyValueType.Reference or (sbyte)WebAssemblyValueType.NullableReference)
                {
                    var refType = ReferenceType.Parse(reader, lead);
                    field.StorageRefType = refType;
                    field.StorageType = refType.ToValueType();
                }
                else
                {
                    field.StorageType = (WebAssemblyValueType)lead;
                }
                break;
        }

        field.IsMutable = reader.ReadVarUInt1() == 1;
        return field;
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString()
    {
        var parameters = this.Parameters;
        var returns = this.Returns;

        var builder = new StringBuilder("Form: ")
            .Append(this.Form.ToString())
            .Append("; Parameters: ")
            ;

        for (var i = 0; i < parameters.Count; i++)
        {
            if (i != 0)
                builder.Append(", ");
            builder.Append(parameters[i]);
        }

        if (returns.Count == 0)
        {
            builder.Append("; (No Returns)");
        }
        else
        {
            builder.Append("; Returns: ");

            for (var i = 0; i < returns.Count; i++)
            {
                if (i != 0)
                    builder.Append(", ");
                builder.Append(returns[i]);
            }
        }

        return builder.ToString();
    }

    internal void WriteTo(Writer writer)
    {
        // Sub-type wrapper: a bare composite (shorthand) is a final sub type with no declared supertypes.  Anything
        // else is emitted with an explicit 0x50 (non-final) / 0x4F (final) sub-type prefix carrying the supertypes.
        var hasSuperTypes = this.SuperTypes is { Count: > 0 };
        if (!this.IsFinal || hasSuperTypes)
        {
            writer.WriteVar((sbyte)(this.IsFinal ? CompositeTypeReader.SubFinalForm : CompositeTypeReader.SubForm));
            var superTypes = this.SuperTypes;
            writer.WriteVar((uint)(superTypes?.Count ?? 0));
            if (superTypes != null)
                foreach (var superType in superTypes)
                    writer.WriteVar(superType);
        }

        writer.WriteVar((sbyte)this.Form);

        switch (this.Form)
        {
            case FunctionType.Struct:
                {
                    var fields = this.Fields ?? [];
                    writer.WriteVar((uint)fields.Count);
                    foreach (var field in fields)
                        WriteField(writer, field);
                }
                break;

            case FunctionType.Array:
                WriteField(writer, this.Fields![0]);
                break;

            default:
                {
                    var parameters = this.Parameters;
                    var returns = this.Returns;

                    writer.WriteVar((uint)parameters.Count);
                    for (var i = 0; i < parameters.Count; i++)
                        WriteValueType(writer, parameters[i], this.parameterRefTypes, i);

                    writer.WriteVar((uint)returns.Count);
                    for (var i = 0; i < returns.Count; i++)
                        WriteValueType(writer, returns[i], this.returnRefTypes, i);
                }
                break;
        }
    }

    private static void WriteField(Writer writer, StructField field)
    {
        switch (field.Packed)
        {
            case PackedType.I8:
                writer.WriteVar((sbyte)CompositeTypeReader.PackedI8Form);
                break;
            case PackedType.I16:
                writer.WriteVar((sbyte)CompositeTypeReader.PackedI16Form);
                break;
            default:
                if (field.StorageRefType is { } refType)
                    refType.WriteTo(writer);
                else
                    writer.WriteVar((sbyte)field.StorageType);
                break;
        }

        writer.WriteVar(field.IsMutable ? 1u : 0u);
    }

    private static void WriteValueType(Writer writer, WebAssemblyValueType valueType, Dictionary<int, ReferenceType>? refTypes, int index)
    {
        if (refTypes is { } map && map.TryGetValue(index, out var refType))
            refType.WriteTo(writer);
        else
            writer.WriteVar((sbyte)valueType);
    }

    /// <summary>
    /// Creates a hash code based on the parameters and returns of this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return (int)this.Form
            ^ HashCode.Combine(this.Parameters.Concat(this.Returns).Select(v => (int)v))
            ^ RefSidecarHash(this.parameterRefTypes)
            ^ RefSidecarHash(this.returnRefTypes)
            ^ (this.IsFinal ? 0 : 0x4F4F4F4F)
            ^ SuperTypesHash(this.SuperTypes)
            ^ FieldsHash(this.Fields);
    }

    private static int SuperTypesHash(IList<uint>? superTypes)
    {
        if (superTypes == null)
            return 0;

        var result = superTypes.Count;
        foreach (var superType in superTypes)
            result = System.HashCode.Combine(result, superType);
        return result;
    }

    private static int FieldsHash(IList<StructField>? fields)
    {
        if (fields == null)
            return 0;

        var result = fields.Count;
        foreach (var field in fields)
            result = System.HashCode.Combine(result, field);
        return result;
    }

    private static bool SuperTypesEqual(IList<uint>? a, IList<uint>? b)
    {
        var countA = a?.Count ?? 0;
        var countB = b?.Count ?? 0;
        if (countA != countB)
            return false;

        for (var i = 0; i < countA; i++)
        {
            if (a![i] != b![i])
                return false;
        }

        return true;
    }

    private static bool FieldsEqual(IList<StructField>? a, IList<StructField>? b)
    {
        if (a == null || b == null)
            return a == null && b == null;
        if (a.Count != b.Count)
            return false;

        for (var i = 0; i < a.Count; i++)
        {
            if (!a[i].Equals(b[i]))
                return false;
        }

        return true;
    }

    private static int RefSidecarHash(Dictionary<int, ReferenceType>? map)
    {
        if (map == null)
            return 0;

        var result = 0;
        foreach (var kv in map)
            result ^= System.HashCode.Combine(kv.Key, kv.Value);
        return result;
    }

    private static bool RefSidecarEquals(Dictionary<int, ReferenceType>? a, Dictionary<int, ReferenceType>? b)
    {
        var countA = a?.Count ?? 0;
        var countB = b?.Count ?? 0;
        if (countA != countB)
            return false;
        if (countA == 0)
            return true;

        foreach (var kv in a!)
        {
            if (!b!.TryGetValue(kv.Key, out var value) || value != kv.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Compares the values of this instance for equality with those of another.
    /// </summary>
    /// <param name="obj">The other instance to compare against.</param>
    /// <returns>True if the two instances have the same values, otherwise false.</returns>
    public override bool Equals(object? obj) => this.Equals(obj as WebAssemblyType);

    /// <summary>
    /// Compares the values of this instance for equality with those of another.
    /// </summary>
    /// <param name="other">The other instance to compare against.</param>
    /// <returns>True if the two instances have the same values, otherwise false.</returns>
    public bool Equals(WebAssemblyType? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other == null)
            return false;

        if (this.Form != other.Form)
            return false;

        if (this.IsFinal != other.IsFinal)
            return false;

        if (!SuperTypesEqual(this.SuperTypes, other.SuperTypes))
            return false;

        if (!FieldsEqual(this.Fields, other.Fields))
            return false;

        var thisReturns = this.Returns;
        var otherReturns = other.Returns;

        if (thisReturns.Count != otherReturns.Count)
            return false;

        var thisParameters = this.Parameters;
        var otherParameters = other.Parameters;

        if (thisParameters.Count != otherParameters.Count)
            return false;

        var count = thisReturns.Count;
        for (var i = 0; i < count; i++)
        {
            if (thisReturns[i] != otherReturns[i])
                return false;
        }

        count = thisParameters.Count;
        for (var i = 0; i < count; i++)
        {
            if (thisParameters[i] != otherParameters[i])
                return false;
        }

        return RefSidecarEquals(this.parameterRefTypes, other.parameterRefTypes)
            && RefSidecarEquals(this.returnRefTypes, other.returnRefTypes);
    }
}
