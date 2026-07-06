using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

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
    /// Throws an <see cref="ImportException"/> if a supplied global import's value type or mutability does not
    /// match what the importing module declared. When both the supplied global and the declaration carry a closed
    /// structural descriptor (the case for reference-typed globals linked from another compiled module), the WASM
    /// type is compared structurally — an immutable global's supply need only be a subtype, a mutable global's type
    /// is invariant — which is finer than the CLR representation can express. When either descriptor is absent
    /// (host-supplied or numeric globals), this falls back to the coarse CLR value-type check.
    /// </summary>
    /// <param name="import">The resolved global import.</param>
    /// <param name="expectedType">The value type the module declared for this import.</param>
    /// <param name="expectedMutable">Whether the module declared this import as mutable.</param>
    /// <param name="expectedDescriptor">The declared type's closed structural descriptor, or null for a numeric global.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    public static void ValidateGlobal(GlobalImport import, WebAssemblyValueType expectedType, bool expectedMutable, string? expectedDescriptor, string module, string field)
    {
        var actualMutable = import.Setter != null;
        if (actualMutable != expectedMutable)
            throw new ImportException(
                $"Import {module}::{field} expected a{(expectedMutable ? " mutable" : "n immutable")} global but the supplied global was {(actualMutable ? "mutable" : "immutable")}.");

        if (import.Descriptor is { } suppliedDescriptor && expectedDescriptor is not null)
        {
            var compatible = expectedMutable
                ? suppliedDescriptor == expectedDescriptor                            // mutable: invariant
                : WasmTypeDescriptor.IsSubtype(suppliedDescriptor, expectedDescriptor); // immutable: supplied <: expected
            if (!compatible)
                throw new ImportException(
                    $"Import {module}::{field} expected a{(expectedMutable ? " mutable" : "n immutable")} global of type {expectedDescriptor} but the supplied global was {suppliedDescriptor}.");
            return;
        }

        if (import.GetterType != expectedType)
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
    /// <param name="expectedIs64">Whether the module declared a 64-bit (memory64) memory.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    public static void ValidateMemory(UnmanagedMemory memory, uint expectedMinimum, uint expectedMaximum, bool expectedHasMaximum, bool expectedIs64, string module, string field)
    {
        // A 64-bit (memory64) import is not satisfied by a 32-bit memory or vice versa; the address types must match.
        if (memory.Is64Bit != expectedIs64)
            throw new ImportException(
                $"Import {module}::{field} expected a{(expectedIs64 ? " 64-bit" : " 32-bit")} memory but the supplied memory was {(memory.Is64Bit ? "64-bit" : "32-bit")}.");

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
    /// <param name="expectedIs64">Whether the module declared a 64-bit (table64) table.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    public static void ValidateTable(TableImport table, uint expectedMinimum, uint expectedMaximum, bool expectedHasMaximum, bool expectedIs64, string module, string field)
    {
        // A 64-bit (table64) import is not satisfied by a 32-bit table or vice versa; the index types must match.
        if (table.Is64Bit != expectedIs64)
            throw new ImportException(
                $"Import {module}::{field} expected a{(expectedIs64 ? " 64-bit" : " 32-bit")} table but the supplied table was {(table.Is64Bit ? "64-bit" : "32-bit")}.");

        // As with memory, the effective lower bound is the table's current length, not its declared initial.
        // An unbounded supplied table can't satisfy a bounded import, hence the explicit null check.
        if (table.Length < expectedMinimum || (expectedHasMaximum && (table.Maximum == null || table.Maximum > expectedMaximum)))
            throw new ImportException(
                $"Import {module}::{field} expected a table of at least {expectedMinimum} entr{(expectedMinimum == 1 ? "y" : "ies")}{(expectedHasMaximum ? $" and at most {expectedMaximum}" : "")} but the supplied table had a range of [{table.Length}, {(object?)table.Maximum ?? "unbounded"}].");
    }

    /// <summary>
    /// Throws an <see cref="ImportException"/> if a supplied table's element type or limits are not compatible with
    /// what the importing module declared. A table's element type is invariant, so when both the supplied table and
    /// the declaration carry a closed structural descriptor (reference-typed tables linked from another compiled
    /// module) the descriptors must match exactly — finer than the CLR <see cref="FunctionTable"/>/<see cref="ExternRefTable"/>
    /// distinction the cast can see. When either descriptor is absent (host-supplied tables), the element type is
    /// assumed already verified by the caller's type cast, as before.
    /// </summary>
    /// <param name="table">The resolved table.</param>
    /// <param name="expectedMinimum">The minimum number of entries the module declared.</param>
    /// <param name="expectedMaximum">The maximum number of entries the module declared; ignored when <paramref name="expectedHasMaximum"/> is false.</param>
    /// <param name="expectedHasMaximum">Whether the module declared a maximum.</param>
    /// <param name="expectedIs64">Whether the module declared a 64-bit (table64) table.</param>
    /// <param name="expectedDescriptor">The declared element type's closed structural descriptor, or null when unavailable.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    public static void ValidateTable(TableImport table, uint expectedMinimum, uint expectedMaximum, bool expectedHasMaximum, bool expectedIs64, string? expectedDescriptor, string module, string field)
    {
        if (table.Is64Bit != expectedIs64)
            throw new ImportException(
                $"Import {module}::{field} expected a{(expectedIs64 ? " 64-bit" : " 32-bit")} table but the supplied table was {(table.Is64Bit ? "64-bit" : "32-bit")}.");

        if (table.Descriptor is { } suppliedDescriptor && expectedDescriptor is not null && suppliedDescriptor != expectedDescriptor)
            throw new ImportException(
                $"Import {module}::{field} expected a table of element type {expectedDescriptor} but the supplied table had element type {suppliedDescriptor}.");

        if (table.Length < expectedMinimum || (expectedHasMaximum && (table.Maximum == null || table.Maximum > expectedMaximum)))
            throw new ImportException(
                $"Import {module}::{field} expected a table of at least {expectedMinimum} entr{(expectedMinimum == 1 ? "y" : "ies")}{(expectedHasMaximum ? $" and at most {expectedMaximum}" : "")} but the supplied table had a range of [{table.Length}, {(object?)table.Maximum ?? "unbounded"}].");
    }

    /// <summary>
    /// Throws an <see cref="ImportException"/> if a supplied exception-tag import's signature does not match what the
    /// importing module declared, otherwise returns the resolved <see cref="ExceptionTag"/> (which preserves the tag's
    /// per-instance identity across linking). Tag-import matching is invariant: when both the supplied tag and the
    /// declaration carry a closed structural descriptor, they must be equal; when either is absent (a host-supplied
    /// tag), the match is accepted (the CLR cast already confirmed the kind).
    /// </summary>
    /// <param name="import">The resolved tag import.</param>
    /// <param name="expectedDescriptor">The declared exception signature's closed structural descriptor, or null.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    /// <returns>The resolved <see cref="ExceptionTag"/>.</returns>
    public static ExceptionTag ValidateTag(TagImport import, string? expectedDescriptor, string module, string field)
    {
        if (import is not ExceptionTag tag)
            throw new ImportException($"Import {module}::{field} expected an exception tag but the supplied import was {import?.GetType().Name ?? "null"}.");

        if (tag.Descriptor is { } suppliedDescriptor && expectedDescriptor is not null && suppliedDescriptor != expectedDescriptor)
            throw new ImportException(
                $"Import {module}::{field} expected a tag of signature {expectedDescriptor} but the supplied tag had signature {suppliedDescriptor}. incompatible import");

        return tag;
    }

    /// <summary>
    /// Throws an <see cref="ImportException"/> if a supplied function import's canonical type is not a subtype of the
    /// type the importing module declared. When the supplied delegate was registered with a canonical type ancestry
    /// (a function linked from another compiled module, WebAssembly 3.0 GC), the declared type must appear on
    /// that ancestry; a host-supplied (unregistered) delegate is accepted, its arity/shape already verified by the
    /// caller's type cast. This is finer than the CLR delegate-shape check, which cannot see structural rec-group
    /// identity (for example a func type that is member 0 of one rec group versus member 1 of another).
    /// </summary>
    /// <param name="function">The resolved function-import delegate.</param>
    /// <param name="expectedDescriptor">The declared import type's closed function-type descriptor.</param>
    /// <param name="module">The import's module name, for diagnostics.</param>
    /// <param name="field">The import's field name, for diagnostics.</param>
    public static void ValidateFunction(Delegate function, string expectedDescriptor, string module, string field)
    {
        if (!WebAssemblyGC.FunctionReferenceMatches(function, WebAssemblyGC.InternFunctionType(expectedDescriptor)))
            throw new ImportException($"Import {module}::{field} has an incompatible function type.");
    }
}
