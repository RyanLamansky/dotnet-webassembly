using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Base class for the WebAssembly 3.0 reference test/cast instructions (<see cref="RefTest"/>, <see cref="RefCast"/>),
/// each carrying a nullability flag (selected by the opcode) and a heap-type target immediate — an abstract
/// <see cref="WebAssemblyHeapType"/> or a concrete type index.
/// </summary>
public abstract class ReferenceTestInstruction : GCInstruction
{
    private WebAssemblyHeapType? heapType = WebAssemblyHeapType.Any;
    private uint? typeIndex;

    private protected ReferenceTestInstruction()
    {
    }

    private protected ReferenceTestInstruction(bool nullable, Reader reader)
    {
        this.Nullable = nullable;
        var heap = reader.ReadHeapTypeS33();
        if (heap >= 0)
        {
            this.typeIndex = (uint)heap;
            this.heapType = null;
        }
        else
        {
            var ht = (WebAssemblyHeapType)(sbyte)heap;
            if (!ht.IsDefined())
                throw new ModuleLoadException($"Unrecognized heap type 0x{(byte)(sbyte)heap:X2}.", reader.Offset);
            this.heapType = ht;
        }
    }

    /// <summary>Whether the target reference type is nullable (the <c>_null</c> opcode variant).</summary>
    public bool Nullable { get; set; }

    /// <summary>The abstract heap type this reference targets, or null when it targets a concrete <see cref="TypeIndex"/>.</summary>
    public WebAssemblyHeapType? HeapType
    {
        get => this.heapType;
        set { this.heapType = value; if (value.HasValue) this.typeIndex = null; }
    }

    /// <summary>The concrete type-section index this reference targets, or null when it targets an abstract <see cref="HeapType"/>.</summary>
    public uint? TypeIndex
    {
        get => this.typeIndex;
        set { this.typeIndex = value; if (value.HasValue) this.heapType = null; }
    }

    private protected sealed override void WriteImmediates(Writer writer) =>
        writer.WriteHeapType(this.heapType, this.typeIndex);

    /// <summary>The <c>(ref null? target)</c> value type this instruction produces or matches against.</summary>
    private protected ValType TargetType => this.typeIndex is { } index
        ? ValType.RefIndexed(this.Nullable, index)
        : ValType.Ref(this.Nullable, this.heapType ?? WebAssemblyHeapType.Any);

    /// <summary>Validates that the type index (when concrete) is within the module's type section.</summary>
    private protected void ValidateTarget(CompilationContext context)
    {
        if (this.typeIndex is { } index)
        {
            var typeCount = context.Types?.Length ?? 0;
            if (index >= (uint)typeCount)
                throw new ModuleLoadException($"unknown type {index}: the module defines only {typeCount} type(s).", 0);
        }
    }
}
