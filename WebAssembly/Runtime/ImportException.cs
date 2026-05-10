using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Runtime;

/// <summary>
/// Indicates that something provided as an import doesn't match the type expected by the WASM.
/// </summary>
public class ImportException : RuntimeException
{
    /// <summary>
    /// Creates a new <see cref="ImportException"/> with a default message.
    /// </summary>
    public ImportException()
        : base("Import type did not match the expected type.")
    {
    }

    /// <summary>
    /// Creates a new <see cref="ImportException"/> with a default message.
    /// </summary>
    public ImportException(string message)
        : base(message)
    {
    }

    internal static void EmitTryCast(ILGenerator il, Type target, CompilerConfiguration configuration)
    {
        il.Emit(OpCodes.Isinst, target);
        il.Emit(OpCodes.Dup);

        var typeCheckPassed = il.DefineLabel();
        il.Emit(OpCodes.Brtrue, typeCheckPassed);

        il.Emit(OpCodes.Newobj, configuration.NeutralizeType(typeof(ImportException)).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 0));
        il.Emit(OpCodes.Throw);

        il.MarkLabel(typeCheckPassed);
    }

    /// <summary>Validates that the provided table's limits satisfy the module's requirements.</summary>
    public static void ValidateTableLimits(FunctionTable table, uint requiredMin, uint requiredMax)
    {
        if (table.Initial < requiredMin || table.Maximum.GetValueOrDefault(uint.MaxValue) > requiredMax)
            throw new ImportException("Incompatible import type: table limits do not match.");
    }

    /// <summary>Validates that the provided externref table's limits satisfy the module's requirements.</summary>
    public static void ValidateTableLimits(ExternRefTable table, uint requiredMin, uint requiredMax)
    {
        if (table.Initial < requiredMin || table.Maximum.GetValueOrDefault(uint.MaxValue) > requiredMax)
            throw new ImportException("Incompatible import type: table limits do not match.");
    }

    /// <summary>Validates that the provided memory's limits satisfy the module's requirements.</summary>
    public static void ValidateMemoryLimits(UnmanagedMemory memory, uint requiredMin, uint requiredMax)
    {
        if (memory.Minimum < requiredMin || memory.Maximum > requiredMax)
            throw new ImportException("Incompatible import type: memory limits do not match.");
    }

    /// <summary>Validates that the provided global's value type matches what the module requires.</summary>
    public static void ValidateGlobalType(GlobalImport global, WebAssemblyValueType requiredType)
    {
        if (global.GetterType != requiredType)
            throw new ImportException($"Incompatible import type: global value type mismatch (required {requiredType}, got {global.GetterType}).");
    }

    /// <summary>Validates that the provided global's mutability matches what the module requires.</summary>
    public static void ValidateGlobalMutability(GlobalImport global, bool requiredMutable)
    {
        var isMutable = global.Setter != null;
        if (isMutable != requiredMutable)
            throw new ImportException($"Incompatible import type: global mutability mismatch (required {(requiredMutable ? "mutable" : "immutable")}, got {(isMutable ? "mutable" : "immutable")}).");
    }
}
