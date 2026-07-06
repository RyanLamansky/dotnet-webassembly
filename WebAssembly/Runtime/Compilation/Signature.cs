using System;
using System.Text;

namespace WebAssembly.Runtime.Compilation;

internal sealed class Signature
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
        : this(returnType, configuration, null)
    {
    }

    // As above, with the per-index composite kinds so a concrete struct/array result resolves to a GC object slot.
    public Signature(ValType returnType, CompilerConfiguration configuration, FunctionType[]? indexedKinds)
        : this()
    {
        this.ReturnTypes = [configuration.NeutralizeType(returnType.ToSystemType(indexedKinds))];
        this.RawReturnTypes = [returnType];
    }

    // True for a function composite type; false for a WASM 3.0 GC composite (struct/array), which is recorded so its
    // index stays addressable but carries no runtime function signature yet.
    public readonly bool IsFunction;

    public Signature(WebAssemblyType type, uint typeIndex, CompilerConfiguration configuration)
        : this(type, typeIndex, configuration, null)
    {
    }

    // indexedKinds carries the composite kind of every declared type index so a concrete struct/array reference in
    // a parameter/result resolves to a GC object slot (object) rather than the Delegate assumed for func indices.
    public Signature(WebAssemblyType type, uint typeIndex, CompilerConfiguration configuration, FunctionType[]? indexedKinds)
    {
        this.TypeIndex = typeIndex;

        if (type.Form != FunctionType.Function)
        {
            // A struct/array composite has no runtime function signature; record an empty placeholder so the type
            // index remains addressable (its CLR representation is the emitted GC class, resolved separately).
            this.ReturnTypes = this.ParameterTypes = [];
            this.RawReturnTypes = this.RawParameterTypes = [];
            return;
        }

        this.IsFunction = true;

        var sourceParameters = type.Parameters;
        var parameters = this.ParameterTypes = new Type[sourceParameters.Count];
        var rawParameters = this.RawParameterTypes = new ValType[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var canonical = rawParameters[i] = ValType.FromValueType(sourceParameters[i], type.GetParameterRefType(i));
            parameters[i] = configuration.NeutralizeType(canonical.ToSystemType(indexedKinds));
        }

        var sourceReturns = type.Returns;
        var returns = this.ReturnTypes = new Type[sourceReturns.Count];
        var rawReturns = this.RawReturnTypes = new ValType[returns.Length];

        for (var i = 0; i < returns.Length; i++)
        {
            var canonical = rawReturns[i] = ValType.FromValueType(sourceReturns[i], type.GetReturnRefType(i));
            returns[i] = configuration.NeutralizeType(canonical.ToSystemType(indexedKinds));
        }
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
