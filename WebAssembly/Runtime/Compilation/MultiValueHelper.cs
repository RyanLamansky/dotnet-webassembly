using System;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation;

static class MultiValueHelper
{
    static readonly Type[] valueTupleTypes =
    [
        typeof(ValueTuple<>),
        typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>),
        typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>),
        typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>),
    ];

    static readonly string[] tupleFieldNames = ["Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7"];

    /// <summary>
    /// Returns the CLR return type: null (void) for 0, identity for 1, ValueTuple for N>1.
    /// </summary>
    public static Type? ClrReturnType(Type[] returnTypes) => returnTypes.Length switch
    {
        0 => null,
        1 => returnTypes[0],
        <= 7 => valueTupleTypes[returnTypes.Length - 1].MakeGenericType(returnTypes),
        _ => throw new NotSupportedException("More than 7 return values are not yet supported.")
    };

    /// <summary>
    /// Returns the type arguments for MakeGenericType on a delegate type:
    /// [..params] for 0 returns, [..params, R1] for 1 return, [..params, ValueTuple&lt;R1,...,RN&gt;] for N>1.
    /// </summary>
    public static Type[] DelegateTypeArgs(Type[] paramTypes, Type[] returnTypes) => returnTypes.Length switch
    {
        0 => paramTypes,
        1 => [.. paramTypes, returnTypes[0]],
        _ => [.. paramTypes, ClrReturnType(returnTypes)!]
    };

    /// <summary>
    /// Emits IL to unpack a ValueTuple on the stack into its individual fields.
    /// The tuple must already be on the evaluation stack.
    /// </summary>
    public static void EmitTupleUnpack(CompilationContext context, Type[] returnTypes)
    {
        var tupleType = ClrReturnType(returnTypes)!;
        var local = context.DeclareLocal(tupleType);
        context.Emit(OpCodes.Stloc, local);

        for (var i = 0; i < returnTypes.Length; i++)
        {
            context.Emit(OpCodes.Ldloca, local);
            var field = tupleType.GetField(tupleFieldNames[i])!;
            context.Emit(OpCodes.Ldfld, field);
        }
    }
}
