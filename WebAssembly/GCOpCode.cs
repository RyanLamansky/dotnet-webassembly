using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebAssembly;

/// <summary>
/// Sub-opcodes of the WebAssembly 3.0 garbage-collection prefix (<see cref="OpCode.GCPrefix"/>, byte <c>0xFB</c>).
/// The value following the prefix is encoded as a LEB128 <c>varuint32</c>.
/// </summary>
public enum GCOpCode : byte
{
    /// <summary>Allocate a struct from operands on the stack (one per field).</summary>
    [OpCodeCharacteristics("struct.new")]
    StructNew = 0x00,

    /// <summary>Allocate a struct with each field set to its default value.</summary>
    [OpCodeCharacteristics("struct.new_default")]
    StructNewDefault = 0x01,

    /// <summary>Read a struct field.</summary>
    [OpCodeCharacteristics("struct.get")]
    StructGet = 0x02,

    /// <summary>Read a packed struct field, sign-extending to i32.</summary>
    [OpCodeCharacteristics("struct.get_s")]
    StructGetSigned = 0x03,

    /// <summary>Read a packed struct field, zero-extending to i32.</summary>
    [OpCodeCharacteristics("struct.get_u")]
    StructGetUnsigned = 0x04,

    /// <summary>Write a struct field.</summary>
    [OpCodeCharacteristics("struct.set")]
    StructSet = 0x05,

    /// <summary>Allocate an array of a given size, filled with an operand value.</summary>
    [OpCodeCharacteristics("array.new")]
    ArrayNew = 0x06,

    /// <summary>Allocate an array of a given size, filled with the element default value.</summary>
    [OpCodeCharacteristics("array.new_default")]
    ArrayNewDefault = 0x07,

    /// <summary>Allocate an array from a fixed number of operands on the stack.</summary>
    [OpCodeCharacteristics("array.new_fixed")]
    ArrayNewFixed = 0x08,

    /// <summary>Allocate an array initialized from a passive data segment.</summary>
    [OpCodeCharacteristics("array.new_data")]
    ArrayNewData = 0x09,

    /// <summary>Allocate an array initialized from a passive element segment.</summary>
    [OpCodeCharacteristics("array.new_elem")]
    ArrayNewElem = 0x0A,

    /// <summary>Read an array element.</summary>
    [OpCodeCharacteristics("array.get")]
    ArrayGet = 0x0B,

    /// <summary>Read a packed array element, sign-extending to i32.</summary>
    [OpCodeCharacteristics("array.get_s")]
    ArrayGetSigned = 0x0C,

    /// <summary>Read a packed array element, zero-extending to i32.</summary>
    [OpCodeCharacteristics("array.get_u")]
    ArrayGetUnsigned = 0x0D,

    /// <summary>Write an array element.</summary>
    [OpCodeCharacteristics("array.set")]
    ArraySet = 0x0E,

    /// <summary>Get the length of an array.</summary>
    [OpCodeCharacteristics("array.len")]
    ArrayLen = 0x0F,

    /// <summary>Fill a range of an array with an operand value.</summary>
    [OpCodeCharacteristics("array.fill")]
    ArrayFill = 0x10,

    /// <summary>Copy a range of elements between two arrays.</summary>
    [OpCodeCharacteristics("array.copy")]
    ArrayCopy = 0x11,

    /// <summary>Copy from a passive data segment into an array.</summary>
    [OpCodeCharacteristics("array.init_data")]
    ArrayInitData = 0x12,

    /// <summary>Copy from a passive element segment into an array.</summary>
    [OpCodeCharacteristics("array.init_elem")]
    ArrayInitElem = 0x13,

    /// <summary>Test whether a reference is of a given (non-null) type.</summary>
    [OpCodeCharacteristics("ref.test")]
    RefTest = 0x14,

    /// <summary>Test whether a reference is of a given (nullable) type.</summary>
    [OpCodeCharacteristics("ref.test_null")]
    RefTestNull = 0x15,

    /// <summary>Cast a reference to a given (non-null) type, trapping on failure.</summary>
    [OpCodeCharacteristics("ref.cast")]
    RefCast = 0x16,

    /// <summary>Cast a reference to a given (nullable) type, trapping on failure.</summary>
    [OpCodeCharacteristics("ref.cast_null")]
    RefCastNull = 0x17,

    /// <summary>Branch to a label if a reference matches a target type, carrying it.</summary>
    [OpCodeCharacteristics("br_on_cast")]
    BranchOnCast = 0x18,

    /// <summary>Branch to a label if a reference does not match a target type, carrying it.</summary>
    [OpCodeCharacteristics("br_on_cast_fail")]
    BranchOnCastFail = 0x19,

    /// <summary>Convert an external reference to an internal (any) reference (identity).</summary>
    [OpCodeCharacteristics("any.convert_extern")]
    AnyConvertExtern = 0x1A,

    /// <summary>Convert an internal (any) reference to an external reference (identity).</summary>
    [OpCodeCharacteristics("extern.convert_any")]
    ExternConvertAny = 0x1B,

    /// <summary>Create an i31 reference from an i32 (masked to 31 bits).</summary>
    [OpCodeCharacteristics("ref.i31")]
    RefI31 = 0x1C,

    /// <summary>Read an i31 reference's payload, sign-extending bit 30.</summary>
    [OpCodeCharacteristics("i31.get_s")]
    I31GetSigned = 0x1D,

    /// <summary>Read an i31 reference's payload (zero-extended).</summary>
    [OpCodeCharacteristics("i31.get_u")]
    I31GetUnsigned = 0x1E,
}

static class GCOpCodeExtensions
{
    private static readonly RegeneratingWeakReference<Dictionary<GCOpCode, string>> opCodeNativeNamesByOpCode = new(
        () => typeof(GCOpCode)
            .GetFields()
            .Where(field => field.IsStatic)
            .Select(field => new KeyValuePair<GCOpCode, string>((GCOpCode)field.GetValue(null)!, field.GetCustomAttribute<OpCodeCharacteristicsAttribute>()!.Name))
            .ToDictionary(kv => kv.Key, kv => kv.Value)
    );

    public static string ToNativeName(this GCOpCode opCode)
    {
        opCodeNativeNamesByOpCode.Reference.TryGetValue(opCode, out var result);
        return result!;
    }
}
