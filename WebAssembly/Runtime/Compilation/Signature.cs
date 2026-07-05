using System;
using System.Text;

namespace WebAssembly.Runtime.Compilation;

internal sealed class Signature : IEquatable<WebAssemblyType>
{
    public readonly uint TypeIndex;
    public readonly Type[] ParameterTypes;
    public readonly ValType[] RawParameterTypes;
    public readonly Type[] ReturnTypes;
    public readonly ValType[] RawReturnTypes;

    private static readonly RegeneratingWeakReference<Signature> empty = new(() => new Signature());

    public static Signature Empty => empty;

    private Signature()
    {
        this.TypeIndex = uint.MaxValue;
        this.ReturnTypes = this.ParameterTypes = [];
        this.RawReturnTypes = this.RawParameterTypes = [];
    }

    public Signature(WebAssemblyValueType returnType, CompilerConfiguration configuration)
        : this()
    {
        this.ReturnTypes = [configuration.NeutralizeType(returnType.ToSystemType())];
        this.RawReturnTypes = [returnType];
    }

    // A synthetic single-result signature used for an inline typed-reference block result
    // (`block (result (ref null? ht))`), routing it through the same multi-value block machinery.
    public Signature(ValType returnType, CompilerConfiguration configuration)
        : this()
    {
        this.ReturnTypes = [configuration.NeutralizeType(returnType.ToSystemType())];
        this.RawReturnTypes = [returnType];
    }

    public Signature(Reader reader, uint typeIndex, CompilerConfiguration configuration)
    {
        this.TypeIndex = typeIndex;

        var form = reader.ReadVarInt7(); //Function Type
        if ((FunctionType)form != FunctionType.Function)
            throw new ModuleLoadException($"GC composite types (form 0x{(byte)form:X2}) are not yet supported (Phase 8).", reader.Offset);

        var parameters = this.ParameterTypes = new Type[reader.ReadVarUInt32()];
        var rawParameters = this.RawParameterTypes = new ValType[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var value = ReferenceType.ReadValueType(reader, out var refType);
            var canonical = rawParameters[i] = ValType.FromValueType(value, refType);
            parameters[i] = configuration.NeutralizeType(canonical.ToSystemType());
        }

        var returns = this.ReturnTypes = new Type[reader.ReadVarUInt32()];
        var rawReturns = this.RawReturnTypes = new ValType[returns.Length];

        for (var i = 0; i < returns.Length; i++)
        {
            var value = ReferenceType.ReadValueType(reader, out var refType);
            var canonical = rawReturns[i] = ValType.FromValueType(value, refType);
            returns[i] = configuration.NeutralizeType(canonical.ToSystemType());
        }
    }

    public bool Equals(WebAssemblyType? other)
    {
        if (other == null)
            return false;

        var thisReturns = this.RawReturnTypes;
        var otherReturns = other.Returns;

        if (thisReturns.Length != otherReturns.Count)
            return false;

        var thisParameters = this.RawParameterTypes;
        var otherParameters = other.Parameters;

        if (thisParameters.Length != otherParameters.Count)
            return false;

        for (var i = 0; i < thisReturns.Length; i++)
        {
            if (thisReturns[i].ToNarrow() != otherReturns[i])
                return false;
        }

        for (var i = 0; i < thisParameters.Length; i++)
        {
            if (thisParameters[i].ToNarrow() != otherParameters[i])
                return false;
        }

        return true;
    }

    public override string ToString()
    {
        var parameters = this.ParameterTypes;
        var returns = this.ReturnTypes;

        var builder = new StringBuilder();

        if (parameters.Length == 0)
        {
            builder.Append("(No Parameters)");
        }
        else
        {
            builder.Append("Parameters: ");

            for (var i = 0; i < parameters.Length; i++)
            {
                if (i != 0)
                    builder.Append(", ");
                builder.Append(parameters[i]);
            }
        }

        if (returns.Length == 0)
        {
            builder.Append("; (No Returns)");
        }
        else
        {
            builder.Append("; Returns: ");

            for (var i = 0; i < returns.Length; i++)
            {
                if (i != 0)
                    builder.Append(", ");
                builder.Append(returns[i]);
            }
        }

        return builder.ToString();
    }
}
