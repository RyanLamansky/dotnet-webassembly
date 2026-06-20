using System;
using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// Maps WebAssembly multi-value (WASM 2.0) function results onto the CLR, which permits only a single
/// return value per method. Zero results map to <c>void</c>, one result maps to the value's type directly,
/// and two-or-more results map to a (possibly nested) <see cref="ValueTuple"/>.
/// </summary>
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
    /// The CLR return type for the provided WebAssembly result types: <see langword="null"/> (void) for none,
    /// the type itself for one, or a <see cref="ValueTuple"/> for two or more.
    /// </summary>
    public static Type? ClrReturnType(Type[] returnTypes) => returnTypes.Length switch
    {
        0 => null,
        1 => returnTypes[0],
        _ => TupleType(returnTypes, 0),
    };

    /// <summary>
    /// The type arguments to apply (via <see cref="Type.MakeGenericType(Type[])"/>) to a delegate type for a
    /// function with the provided parameters and results: <c>[..params]</c> for zero results, <c>[..params, R]</c>
    /// for one, and <c>[..params, ValueTuple&lt;R1,…,RN&gt;]</c> for more. For two-or-more results the delegate
    /// type itself should be requested as if it had a single result (the tuple).
    /// </summary>
    public static Type[] DelegateTypeArgs(Type[] paramTypes, Type[] returnTypes) => returnTypes.Length switch
    {
        0 => paramTypes,
        1 => [.. paramTypes, returnTypes[0]],
        _ => [.. paramTypes, TupleType(returnTypes, 0)],
    };

    /// <summary>
    /// The (possibly nested) <see cref="ValueTuple"/> type spanning <paramref name="returnTypes"/> from
    /// <paramref name="start"/> to the end.
    /// </summary>
    static Type TupleType(Type[] returnTypes, int start)
    {
        var remaining = returnTypes.Length - start;
        if (remaining <= 7)
        {
            var args = new Type[remaining];
            Array.Copy(returnTypes, start, args, 0, remaining);
            return valueTupleTypes[remaining - 1].MakeGenericType(args);
        }

        var ctorArgs = new Type[8];
        Array.Copy(returnTypes, start, ctorArgs, 0, 7);
        ctorArgs[7] = TupleType(returnTypes, start + 7);
        return valueTupleTypes[7].MakeGenericType(ctorArgs);
    }

    /// <summary>
    /// Emits IL that packs the top <c>returnTypes.Length</c> values already on the evaluation stack into a
    /// single <see cref="ValueTuple"/>, leaving that tuple on the stack. No locals are used: because the
    /// WebAssembly operand stack maps directly onto the CLR evaluation stack, the values are already in the
    /// order the tuple constructors consume them (each constructor takes the topmost values, with the nested
    /// "Rest" tuple — built first — as its final argument).
    /// </summary>
    public static void EmitTuplePack(CompilationContext context, Type[] returnTypes)
        => EmitTuplePack(context, returnTypes, 0);

    static void EmitTuplePack(CompilationContext context, Type[] returnTypes, int start)
    {
        var remaining = returnTypes.Length - start;
        if (remaining <= 7)
        {
            var args = new Type[remaining];
            Array.Copy(returnTypes, start, args, 0, remaining);
            var tupleType = valueTupleTypes[remaining - 1].MakeGenericType(args);
            context.Emit(OpCodes.Newobj, tupleType.GetConstructor(args)!);
            return;
        }

        // The values for the "Rest" tuple (start + 7 onward) are on top, so build it first; this leaves the
        // first seven values below it, exactly as the outer constructor expects.
        EmitTuplePack(context, returnTypes, start + 7);

        var ctorArgs = new Type[8];
        Array.Copy(returnTypes, start, ctorArgs, 0, 7);
        ctorArgs[7] = TupleType(returnTypes, start + 7);
        var outerType = valueTupleTypes[7].MakeGenericType(ctorArgs);
        context.Emit(OpCodes.Newobj, outerType.GetConstructor(ctorArgs)!);
    }

    /// <summary>
    /// Emits IL that unpacks the <see cref="ValueTuple"/> currently on top of the evaluation stack into its
    /// constituent values, in order (so the final result is on top). A local is required to address the tuple's
    /// fields; nested "Rest" tuples are unpacked recursively.
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
            context.Emit(OpCodes.Ldfld, tupleType.GetField(tupleFieldNames[i])!);
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
}
