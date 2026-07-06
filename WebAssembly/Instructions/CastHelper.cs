using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Shared lowering for the WebAssembly 3.0 reference test/cast instructions (<see cref="RefTest"/>,
/// <see cref="RefCast"/>, <see cref="BranchOnCast"/>, <see cref="BranchOnCastFail"/>): classifying a heap-type
/// target into its runtime check and emitting the type-test (producing an <c>i32</c>) or checked cast IL.
/// </summary>
static class CastHelper
{
    private static readonly ConstructorInfo invalidCastExceptionConstructor =
        typeof(InvalidCastException).GetConstructor(Type.EmptyTypes)!;

    /// <summary>How a cast/test target lowers at run time.</summary>
    public enum TargetKind
    {
        /// <summary>A concrete class check via <c>isinst</c>/<c>castclass</c> against <c>ClrType</c>.</summary>
        IsInst,

        /// <summary>The top of a hierarchy (<c>any</c>/<c>extern</c>/<c>exn</c>): every non-null value matches.</summary>
        Top,

        /// <summary>A bottom heap type (<c>none</c>/<c>nofunc</c>/<c>noextern</c>/<c>noexn</c>): no value matches.</summary>
        Bottom,

        /// <summary>A concrete function index: checked by canonical function-type identity (WASM 3.0, design §10).</summary>
        FuncType,
    }

    /// <summary>
    /// Classifies a heap-type target (a concrete type index or an abstract heap type) into its runtime check.
    /// <c>Descriptor</c> is the closed function-type descriptor for a concrete-function-index (<see cref="TargetKind.FuncType"/>) target.
    /// </summary>
    public static (TargetKind Kind, Type? ClrType, string? Descriptor) Classify(
        CompilationContext context, WebAssemblyHeapType? heapType, uint? typeIndex)
    {
        if (typeIndex is { } index)
        {
            // A concrete struct/array index checks its emitted class.
            if (context.GCTypes is { } emitter && emitter.Has(index))
                return (TargetKind.IsInst, emitter.GetClass(index), null);

            // A concrete function index is checked by canonical function-type identity via the registry, not just
            // Delegate shape (design §10): two differently-shaped rec groups can share the same CLR delegate type.
            if (context.CanonicalTypes is { } map && index < (uint)map.Descriptor.Length)
                return (TargetKind.FuncType, null, map.Descriptor[index]);

            return (TargetKind.IsInst, context.Configuration.NeutralizeType(typeof(Delegate)), null);
        }

        return heapType switch
        {
            WebAssemblyHeapType.Func => (TargetKind.IsInst, context.Configuration.NeutralizeType(typeof(Delegate)), null),
            WebAssemblyHeapType.Struct => (TargetKind.IsInst, context.Configuration.NeutralizeType(typeof(WasmStructObject)), null),
            WebAssemblyHeapType.Array => (TargetKind.IsInst, context.Configuration.NeutralizeType(typeof(WasmArrayObject)), null),
            WebAssemblyHeapType.I31 => (TargetKind.IsInst, context.Configuration.NeutralizeType(typeof(WasmI31)), null),
            WebAssemblyHeapType.Eq => (TargetKind.IsInst, context.Configuration.NeutralizeType(typeof(IWasmEq)), null),
            WebAssemblyHeapType.Any or WebAssemblyHeapType.Extern or WebAssemblyHeapType.Exn => (TargetKind.Top, null, null),
            _ => (TargetKind.Bottom, null, null), // none / nofunc / noextern / noexn
        };
    }

    private static MethodInfo WasmGCMethod(CompilationContext context, string name) =>
        context.Configuration.NeutralizeType(typeof(WasmGC)).GetMethod(name)!;

    /// <summary>
    /// Emits a type test: consumes the reference on top of the stack and pushes an <c>i32</c> (1 when the value
    /// matches <c>(ref null? target)</c>, else 0). <paramref name="nullable"/> selects whether null passes.
    /// </summary>
    public static void EmitTest(CompilationContext context, bool nullable, TargetKind kind, Type? clrType, string? descriptor = null)
    {
        switch (kind)
        {
            case TargetKind.FuncType:
                if (nullable)
                {
                    var isNull = context.DefineLabel();
                    var done = context.DefineLabel();
                    context.Emit(OpCodes.Dup);
                    context.Emit(OpCodes.Brfalse, isNull);
                    context.Emit(OpCodes.Ldstr, descriptor!);
                    context.Emit(OpCodes.Call, WasmGCMethod(context, nameof(WasmGC.IsFunctionReferenceOfType)));
                    context.Emit(OpCodes.Br, done);
                    context.MarkLabel(isNull);
                    context.Emit(OpCodes.Pop);
                    context.Emit(OpCodes.Ldc_I4_1);
                    context.MarkLabel(done);
                }
                else
                {
                    // A null value is not a Delegate, so the helper returns false — the correct non-null result.
                    context.Emit(OpCodes.Ldstr, descriptor!);
                    context.Emit(OpCodes.Call, WasmGCMethod(context, nameof(WasmGC.IsFunctionReferenceOfType)));
                }
                break;

            case TargetKind.IsInst:
                if (nullable)
                {
                    // null matches; otherwise 1 iff the value is an instance of the target class.
                    var isNull = context.DefineLabel();
                    var done = context.DefineLabel();
                    context.Emit(OpCodes.Dup);
                    context.Emit(OpCodes.Brfalse, isNull);
                    context.Emit(OpCodes.Isinst, clrType!);
                    context.Emit(OpCodes.Ldnull);
                    context.Emit(OpCodes.Cgt_Un);
                    context.Emit(OpCodes.Br, done);
                    context.MarkLabel(isNull);
                    context.Emit(OpCodes.Pop);
                    context.Emit(OpCodes.Ldc_I4_1);
                    context.MarkLabel(done);
                }
                else
                {
                    // A null value's isinst is null → 0, which is the correct non-null-test result.
                    context.Emit(OpCodes.Isinst, clrType!);
                    context.Emit(OpCodes.Ldnull);
                    context.Emit(OpCodes.Cgt_Un);
                }
                break;

            case TargetKind.Top:
                if (nullable)
                {
                    context.Emit(OpCodes.Pop);
                    context.Emit(OpCodes.Ldc_I4_1);
                }
                else
                {
                    context.Emit(OpCodes.Ldnull);
                    context.Emit(OpCodes.Cgt_Un);
                }
                break;

            default: // Bottom
                if (nullable)
                {
                    context.Emit(OpCodes.Ldnull);
                    context.Emit(OpCodes.Ceq);
                }
                else
                {
                    context.Emit(OpCodes.Pop);
                    context.Emit(OpCodes.Ldc_I4_0);
                }
                break;
        }
    }

    /// <summary>
    /// Emits a checked cast: consumes the reference on top of the stack and leaves it (as the target type),
    /// trapping with <see cref="InvalidCastException"/> ("cast failure") when the value is not of the target type.
    /// A non-null cast of null is itself a cast failure (a null does not inhabit a non-null type), so it also
    /// traps with <see cref="InvalidCastException"/> — unlike <c>ref.as_non_null</c>, which raises "null reference".
    /// </summary>
    public static void EmitCast(CompilationContext context, bool nullable, TargetKind kind, Type? clrType, string? descriptor = null)
    {
        switch (kind)
        {
            case TargetKind.FuncType:
                if (nullable)
                {
                    // null passes through; a non-null value must match the canonical function type.
                    var done = context.DefineLabel();
                    context.Emit(OpCodes.Dup);
                    context.Emit(OpCodes.Brfalse, done);
                    context.Emit(OpCodes.Ldstr, descriptor!);
                    context.Emit(OpCodes.Call, WasmGCMethod(context, nameof(WasmGC.CastFunctionReference)));
                    context.MarkLabel(done);
                }
                else
                {
                    // A null (or mismatched) value traps "cast failure"; a match returns the value unchanged.
                    context.Emit(OpCodes.Ldstr, descriptor!);
                    context.Emit(OpCodes.Call, WasmGCMethod(context, nameof(WasmGC.CastFunctionReference)));
                }
                break;

            case TargetKind.IsInst:
                if (!nullable)
                    EmitNonNullGuard(context);
                // castclass passes null through (harmless after the guard) and throws InvalidCastException otherwise.
                context.Emit(OpCodes.Castclass, clrType!);
                break;

            case TargetKind.Top:
                if (!nullable)
                    EmitNonNullGuard(context);
                // The value already inhabits the hierarchy top; nothing more to check.
                break;

            default: // Bottom: no value (null or otherwise) matches.
                if (nullable)
                {
                    // null passes; a non-null value is a cast failure.
                    var done = context.DefineLabel();
                    context.Emit(OpCodes.Dup);
                    context.Emit(OpCodes.Brfalse, done);
                    context.Emit(OpCodes.Newobj, invalidCastExceptionConstructor);
                    context.Emit(OpCodes.Throw);
                    context.MarkLabel(done);
                }
                else
                {
                    // Every value (including null) fails; the helper always throws but keeps the IL stack balanced.
                    context.Emit(OpCodes.Call, context.Configuration.NeutralizeType(typeof(WasmGC))
                        .GetMethod(nameof(WasmGC.FailCast), [typeof(object)])!);
                }
                break;
        }
    }

    // dup; brtrue ok; throw new InvalidCastException(); ok:  — the non-null cast's null-is-a-cast-failure trap.
    private static void EmitNonNullGuard(CompilationContext context)
    {
        var ok = context.DefineLabel();
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Brtrue, ok);
        context.Emit(OpCodes.Newobj, invalidCastExceptionConstructor);
        context.Emit(OpCodes.Throw);
        context.MarkLabel(ok);
    }
}
