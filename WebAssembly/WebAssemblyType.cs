using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace WebAssembly;

/// <summary>
/// Describes the signature of a function.
/// </summary>
public class WebAssemblyType : IEquatable<WebAssemblyType>
{
    /// <summary>
    /// The type of function.  The only accepted value in the initial binary format is <see cref="FunctionType.Function"/>, which is the default.
    /// </summary>
    public FunctionType Form { get; set; }

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

    internal WebAssemblyType(Reader reader)
    {
        this.Form = (FunctionType)reader.ReadVarInt7();
        if (this.Form != FunctionType.Function)
            throw new ModuleLoadException($"GC composite types (form 0x{(byte)(sbyte)this.Form:X2}) are not yet supported (Phase 8).", reader.Offset);

        int count;

        count = checked((int)reader.ReadVarUInt32());
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
        var parameters = this.Parameters;
        var returns = this.Returns;

        writer.WriteVar((sbyte)this.Form);

        writer.WriteVar((uint)parameters.Count);
        for (var i = 0; i < parameters.Count; i++)
            WriteValueType(writer, parameters[i], this.parameterRefTypes, i);

        writer.WriteVar((uint)returns.Count);
        for (var i = 0; i < returns.Count; i++)
            WriteValueType(writer, returns[i], this.returnRefTypes, i);
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
            ^ RefSidecarHash(this.returnRefTypes);
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
