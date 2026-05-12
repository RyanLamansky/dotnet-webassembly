using System;
using System.Linq;
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
        typeof(ValueTuple<,,,,,,,>),
    ];

    static readonly string[] tupleFieldNames = ["Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7"];

    /// <summary>
    /// Returns the CLR return type: null (void) for 0, identity for 1, ValueTuple for N>1.
    /// </summary>
    public static Type? ClrReturnType(Type[] returnTypes) => returnTypes.Length switch
    {
        0 => null,
        1 => returnTypes[0],
        _ => ClrTupleType(returnTypes)
    };

    static Type ClrTupleType(Type[] returnTypes) => returnTypes.Length <= 7
        ? valueTupleTypes[returnTypes.Length - 1].MakeGenericType(returnTypes)
        : valueTupleTypes[valueTupleTypes.Length - 1].MakeGenericType(
            [.. returnTypes.Take(7), ClrTupleType([.. returnTypes.Skip(7)])]);

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

        EmitTupleUnpack(context, tupleType, returnTypes.Length, local);
    }

    static void EmitTupleUnpack(CompilationContext context, Type tupleType, int valueCount, LocalBuilder tupleLocal)
    {
        var directValues = Math.Min(valueCount, 7);
        for (var i = 0; i < directValues; i++)
        {
            context.Emit(OpCodes.Ldloca, tupleLocal);
            var field = tupleType.GetField(tupleFieldNames[i])!;
            context.Emit(OpCodes.Ldfld, field);
        }

        if (valueCount <= 7)
            return;

        var restField = tupleType.GetField("Rest")!;
        var restLocal = context.DeclareLocal(restField.FieldType);
        context.Emit(OpCodes.Ldloca, tupleLocal);
        context.Emit(OpCodes.Ldfld, restField);
        context.Emit(OpCodes.Stloc, restLocal);
        EmitTupleUnpack(context, restField.FieldType, valueCount - 7, restLocal);
    }

    public static void EmitTuplePack(CompilationContext context, Type[] returnTypes, LocalBuilder[] valueLocals)
    {
        EmitTuplePack(context, returnTypes, valueLocals, 0);
    }

    static void EmitTuplePack(CompilationContext context, Type[] returnTypes, LocalBuilder[] valueLocals, int startIndex)
    {
        var remaining = returnTypes.Length - startIndex;
        var directValues = Math.Min(remaining, 7);
        for (var i = 0; i < directValues; i++)
            context.Emit(OpCodes.Ldloc, valueLocals[startIndex + i]);

        Type tupleType;
        Type[] ctorArgs;
        if (remaining <= 7)
        {
            ctorArgs = [.. returnTypes.Skip(startIndex)];
            tupleType = valueTupleTypes[remaining - 1].MakeGenericType(ctorArgs);
        }
        else
        {
            EmitTuplePack(context, returnTypes, valueLocals, startIndex + 7);
            var restType = ClrTupleType([.. returnTypes.Skip(startIndex + 7)]);
            ctorArgs = [.. returnTypes.Skip(startIndex).Take(7), restType];
            tupleType = valueTupleTypes[valueTupleTypes.Length - 1].MakeGenericType(ctorArgs);
        }

        var ctor = tupleType.GetConstructor(ctorArgs)!;
        context.Emit(OpCodes.Newobj, ctor);
    }
}
