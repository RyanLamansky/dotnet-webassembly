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

    // The Validate* helpers below are invoked from the instance constructor emitted by Compile, after the
    // import has been resolved and its .NET subtype confirmed. They enforce WASM-level type compatibility
    // (which the Isinst cast above can't see): a global's value type and mutability, and the limits of a
    // memory or table. They are public because the emitted instance lives in a separate dynamic assembly,
    // which cannot call internal members. The maximum is passed as (value, present) rather than a Nullable
    // so the emitter can push plain int/bool constants. A registered module's exports reach these as the
    // same RuntimeImport subtypes the host supplies, so both linking paths are validated uniformly.

    /// <summary>
    /// Throws an <see cref="ImportException"/> if a supplied global import's value type or mutability does
    /// not match what the importing module declared.
    /// </summary>
    /// <param name="import">The resolved global import.</param>
    /// <param name="expectedType">The value type the module declared for this import.</param>
    /// <param name="expectedMutable">Whether the module declared this import as mutable.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    public static void ValidateGlobal(GlobalImport import, WebAssemblyValueType expectedType, bool expectedMutable, string module, string field)
    {
        var actualMutable = import.Setter != null;
        if (import.GetterType != expectedType || actualMutable != expectedMutable)
            throw new ImportException(
                $"Import {module}::{field} expected a {(expectedMutable ? "mutable " : "")}{expectedType} global but the supplied global was {(actualMutable ? "mutable " : "")}{import.GetterType}.");
    }

    /// <summary>
    /// Throws an <see cref="ImportException"/> if a supplied memory's limits are not compatible with what the
    /// importing module declared (the memory must be at least as large as required and no larger than allowed).
    /// </summary>
    /// <param name="memory">The resolved memory.</param>
    /// <param name="expectedMinimum">The minimum number of pages the module declared.</param>
    /// <param name="expectedMaximum">The maximum number of pages the module declared; ignored when <paramref name="expectedHasMaximum"/> is false.</param>
    /// <param name="expectedHasMaximum">Whether the module declared a maximum.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    public static void ValidateMemory(UnmanagedMemory memory, uint expectedMinimum, uint expectedMaximum, bool expectedHasMaximum, string module, string field)
    {
        // The effective lower bound is the memory's current size, not its declared minimum: a memory grown
        // past its minimum satisfies an import that requires that larger size. UnmanagedMemory.Maximum is
        // always concrete (an unbounded memory reports the page cap), so a memory with no declared maximum
        // correctly fails an import that imposes one.
        if (memory.Current < expectedMinimum || (expectedHasMaximum && memory.Maximum > expectedMaximum))
            throw new ImportException(
                $"Import {module}::{field} expected a memory of at least {expectedMinimum} page(s){(expectedHasMaximum ? $" and at most {expectedMaximum}" : "")} but the supplied memory had a range of [{memory.Current}, {memory.Maximum}].");
    }

    /// <summary>
    /// Throws an <see cref="ImportException"/> if a supplied table's limits are not compatible with what the
    /// importing module declared. The element type is assumed already verified by the caller's type cast.
    /// </summary>
    /// <param name="table">The resolved table.</param>
    /// <param name="expectedMinimum">The minimum number of entries the module declared.</param>
    /// <param name="expectedMaximum">The maximum number of entries the module declared; ignored when <paramref name="expectedHasMaximum"/> is false.</param>
    /// <param name="expectedHasMaximum">Whether the module declared a maximum.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    public static void ValidateTable(TableImport table, uint expectedMinimum, uint expectedMaximum, bool expectedHasMaximum, string module, string field)
    {
        // As with memory, the effective lower bound is the table's current length, not its declared initial.
        // An unbounded supplied table can't satisfy a bounded import, hence the explicit null check.
        if (table.Length < expectedMinimum || (expectedHasMaximum && (table.Maximum == null || table.Maximum > expectedMaximum)))
            throw new ImportException(
                $"Import {module}::{field} expected a table of at least {expectedMinimum} entr{(expectedMinimum == 1 ? "y" : "ies")}{(expectedHasMaximum ? $" and at most {expectedMaximum}" : "")} but the supplied table had a range of [{table.Length}, {(object?)table.Maximum ?? "unbounded"}].");
    }
}
