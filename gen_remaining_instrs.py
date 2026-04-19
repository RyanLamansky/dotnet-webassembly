import os

INSTR_DEST = r"C:\Users\mreed\source\repos\dotnet-webassembly\WebAssembly\Instructions"
TEST_DEST = r"C:\Users\mreed\source\repos\dotnet-webassembly\WebAssembly.Tests\Instructions"

def make_instr(class_name, simd_op, method_ref, base_class):
    return f"""using System;
using System.Reflection;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>{simd_op} instruction.</summary>
public class {class_name} : {base_class}, IEquatable<{class_name}>
{{
    /// <summary>Always <see cref="SimdOpCode.{simd_op}"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.{simd_op};

    internal override RegeneratingWeakReference<MethodInfo> Method => V128Helper.{method_ref};

    /// <summary>Creates a new <see cref="{class_name}"/> instance.</summary>
    public {class_name}() {{ }}

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is {class_name};
    /// <inheritdoc/>
    public bool Equals({class_name}? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is {class_name};
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.{simd_op};
}}
"""

def make_test_binary(class_name, a_bytes, b_bytes, expected_byte0):
    a_str = ", ".join(str(x) for x in a_bytes)
    b_str = ", ".join(str(x) for x in b_bytes)
    export_class = class_name + "Export"
    return f"""using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="{class_name}"/> instruction.</summary>
[TestClass]
public class {class_name}Tests
{{
    /// <summary>Export for {class_name} test.</summary>
    public abstract class {export_class}
    {{
        /// <summary>Returns the byte at the given offset of the result.</summary>
        public abstract int GetByte(int offset);
    }}

    private static Module BuildModule()
    {{
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {{
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        }});
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export {{ Name = nameof({export_class}.GetByte) }});
        module.Codes.Add(new FunctionBody
        {{
            Code =
            [
                new Int32Constant(0),
                new V128Const {{ Value = [{a_str}] }},
                new V128Const {{ Value = [{b_str}] }},
                new {class_name}(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        }});
        return module;
    }}

    /// <summary>Verifies {class_name} produces correct results.</summary>
    [TestMethod]
    public void {class_name}_IsCorrect()
    {{
        var compiled = BuildModule().ToInstance<{export_class}>();
        Assert.AreEqual({expected_byte0}, compiled.Exports.GetByte(0));
    }}
}}
"""

def make_test_unary(class_name, a_bytes, expected_byte0):
    a_str = ", ".join(str(x) for x in a_bytes)
    export_class = class_name + "Export"
    return f"""using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="{class_name}"/> instruction.</summary>
[TestClass]
public class {class_name}Tests
{{
    /// <summary>Export for {class_name} test.</summary>
    public abstract class {export_class}
    {{
        /// <summary>Returns the byte at the given offset of the result.</summary>
        public abstract int GetByte(int offset);
    }}

    private static Module BuildModule()
    {{
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {{
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        }});
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export {{ Name = nameof({export_class}.GetByte) }});
        module.Codes.Add(new FunctionBody
        {{
            Code =
            [
                new Int32Constant(0),
                new V128Const {{ Value = [{a_str}] }},
                new {class_name}(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        }});
        return module;
    }}

    /// <summary>Verifies {class_name} produces correct results.</summary>
    [TestMethod]
    public void {class_name}_IsCorrect()
    {{
        var compiled = BuildModule().ToInstance<{export_class}>();
        Assert.AreEqual({expected_byte0}, compiled.Exports.GetByte(0));
    }}
}}
"""

def make_test_v128_to_i32(class_name, a_bytes, expected_i32):
    a_str = ", ".join(str(x) for x in a_bytes)
    export_class = class_name + "Export"
    return f"""using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="{class_name}"/> instruction.</summary>
[TestClass]
public class {class_name}Tests
{{
    /// <summary>Export for {class_name} test.</summary>
    public abstract class {export_class}
    {{
        /// <summary>Returns the i32 result.</summary>
        public abstract int GetResult();
    }}

    private static Module BuildModule()
    {{
        var module = new Module();
        module.Types.Add(new WebAssemblyType {{ Returns = [WebAssemblyValueType.Int32] }});
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export {{ Name = nameof({export_class}.GetResult) }});
        module.Codes.Add(new FunctionBody
        {{
            Code =
            [
                new V128Const {{ Value = [{a_str}] }},
                new {class_name}(),
                new End(),
            ],
        }});
        return module;
    }}

    /// <summary>Verifies {class_name} produces correct results.</summary>
    [TestMethod]
    public void {class_name}_IsCorrect()
    {{
        var compiled = BuildModule().ToInstance<{export_class}>();
        Assert.AreEqual({expected_i32}, compiled.Exports.GetResult());
    }}
}}
"""

def make_test_shift(class_name, a_bytes, shift, expected_byte0):
    a_str = ", ".join(str(x) for x in a_bytes)
    export_class = class_name + "Export"
    return f"""using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="{class_name}"/> instruction.</summary>
[TestClass]
public class {class_name}Tests
{{
    /// <summary>Export for {class_name} test.</summary>
    public abstract class {export_class}
    {{
        /// <summary>Returns the byte at the given offset of the result.</summary>
        public abstract int GetByte(int offset);
    }}

    private static Module BuildModule()
    {{
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {{
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        }});
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export {{ Name = nameof({export_class}.GetByte) }});
        module.Codes.Add(new FunctionBody
        {{
            Code =
            [
                new Int32Constant(0),
                new V128Const {{ Value = [{a_str}] }},
                new Int32Constant({shift}),
                new {class_name}(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        }});
        return module;
    }}

    /// <summary>Verifies {class_name} produces correct results.</summary>
    [TestMethod]
    public void {class_name}_IsCorrect()
    {{
        var compiled = BuildModule().ToInstance<{export_class}>();
        Assert.AreEqual({expected_byte0}, compiled.Exports.GetByte(0));
    }}
}}
"""

# (class_name, simd_op, method_ref, base_class, test_kind, test_args)
# test_kinds: "binary"=(a16,b16,exp0), "unary"=(a16,exp0), "v128_i32"=(a16,exp_i32), "shift"=(a16,shift_amt,exp0)

all16z = [0]*16
all16f = [0xFF]*16

instructions = [
    # --- Comparisons (binary → v128 mask) ---
    # i8x16 eq/ne/lt/gt/le/ge: result is 0xFF (all ones) per lane if true, else 0x00
    ("Int8x16Equal",                    "Int8x16Equal",                    "Int8x16EqualMethod",                    "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int8x16NotEqual",                 "Int8x16NotEqual",                 "Int8x16NotEqualMethod",                 "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [2]+[0]*15, 0xFF)),
    ("Int8x16LessThanSigned",           "Int8x16LessThanSigned",           "Int8x16LtSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [2]+[0]*15, 0xFF)),
    ("Int8x16LessThanUnsigned",         "Int8x16LessThanUnsigned",         "Int8x16LtUMethod",                      "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [2]+[0]*15, 0xFF)),
    ("Int8x16GreaterThanSigned",        "Int8x16GreaterThanSigned",        "Int8x16GtSMethod",                      "SimdBinaryV128Instruction", "binary",   ([2]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int8x16GreaterThanUnsigned",      "Int8x16GreaterThanUnsigned",      "Int8x16GtUMethod",                      "SimdBinaryV128Instruction", "binary",   ([2]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int8x16LessThanOrEqualSigned",    "Int8x16LessThanOrEqualSigned",    "Int8x16LeSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int8x16LessThanOrEqualUnsigned",  "Int8x16LessThanOrEqualUnsigned",  "Int8x16LeUMethod",                      "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int8x16GreaterThanOrEqualSigned", "Int8x16GreaterThanOrEqualSigned", "Int8x16GeSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int8x16GreaterThanOrEqualUnsigned","Int8x16GreaterThanOrEqualUnsigned","Int8x16GeUMethod",                    "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [1]+[0]*15, 0xFF)),

    ("Int16x8Equal",                    "Int16x8Equal",                    "Int16x8EqualMethod",                    "SimdBinaryV128Instruction", "binary",   ([1,0]+[0]*14, [1,0]+[0]*14, 0xFF)),
    ("Int16x8NotEqual",                 "Int16x8NotEqual",                 "Int16x8NotEqualMethod",                 "SimdBinaryV128Instruction", "binary",   ([1,0]+[0]*14, [2,0]+[0]*14, 0xFF)),
    ("Int16x8LessThanSigned",           "Int16x8LessThanSigned",           "Int16x8LtSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0]+[0]*14, [2,0]+[0]*14, 0xFF)),
    ("Int16x8LessThanUnsigned",         "Int16x8LessThanUnsigned",         "Int16x8LtUMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0]+[0]*14, [2,0]+[0]*14, 0xFF)),
    ("Int16x8GreaterThanSigned",        "Int16x8GreaterThanSigned",        "Int16x8GtSMethod",                      "SimdBinaryV128Instruction", "binary",   ([2,0]+[0]*14, [1,0]+[0]*14, 0xFF)),
    ("Int16x8GreaterThanUnsigned",      "Int16x8GreaterThanUnsigned",      "Int16x8GtUMethod",                      "SimdBinaryV128Instruction", "binary",   ([2,0]+[0]*14, [1,0]+[0]*14, 0xFF)),
    ("Int16x8LessThanOrEqualSigned",    "Int16x8LessThanOrEqualSigned",    "Int16x8LeSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0]+[0]*14, [1,0]+[0]*14, 0xFF)),
    ("Int16x8LessThanOrEqualUnsigned",  "Int16x8LessThanOrEqualUnsigned",  "Int16x8LeUMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0]+[0]*14, [1,0]+[0]*14, 0xFF)),
    ("Int16x8GreaterThanOrEqualSigned", "Int16x8GreaterThanOrEqualSigned", "Int16x8GeSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0]+[0]*14, [1,0]+[0]*14, 0xFF)),
    ("Int16x8GreaterThanOrEqualUnsigned","Int16x8GreaterThanOrEqualUnsigned","Int16x8GeUMethod",                    "SimdBinaryV128Instruction", "binary",   ([1,0]+[0]*14, [1,0]+[0]*14, 0xFF)),

    ("Int32x4Equal",                    "Int32x4Equal",                    "Int32x4EqualMethod",                    "SimdBinaryV128Instruction", "binary",   ([1,0,0,0]+[0]*12, [1,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4NotEqual",                 "Int32x4NotEqual",                 "Int32x4NotEqualMethod",                 "SimdBinaryV128Instruction", "binary",   ([1,0,0,0]+[0]*12, [2,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4LessThanSigned",           "Int32x4LessThanSigned",           "Int32x4LtSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0,0,0]+[0]*12, [2,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4LessThanUnsigned",         "Int32x4LessThanUnsigned",         "Int32x4LtUMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0,0,0]+[0]*12, [2,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4GreaterThanSigned",        "Int32x4GreaterThanSigned",        "Int32x4GtSMethod",                      "SimdBinaryV128Instruction", "binary",   ([2,0,0,0]+[0]*12, [1,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4GreaterThanUnsigned",      "Int32x4GreaterThanUnsigned",      "Int32x4GtUMethod",                      "SimdBinaryV128Instruction", "binary",   ([2,0,0,0]+[0]*12, [1,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4LessThanOrEqualSigned",    "Int32x4LessThanOrEqualSigned",    "Int32x4LeSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0,0,0]+[0]*12, [1,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4LessThanOrEqualUnsigned",  "Int32x4LessThanOrEqualUnsigned",  "Int32x4LeUMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0,0,0]+[0]*12, [1,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4GreaterThanOrEqualSigned", "Int32x4GreaterThanOrEqualSigned", "Int32x4GeSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1,0,0,0]+[0]*12, [1,0,0,0]+[0]*12, 0xFF)),
    ("Int32x4GreaterThanOrEqualUnsigned","Int32x4GreaterThanOrEqualUnsigned","Int32x4GeUMethod",                    "SimdBinaryV128Instruction", "binary",   ([1,0,0,0]+[0]*12, [1,0,0,0]+[0]*12, 0xFF)),

    ("Int64x2Equal",                    "Int64x2Equal",                    "Int64x2EqualMethod",                    "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int64x2NotEqual",                 "Int64x2NotEqual",                 "Int64x2NotEqualMethod",                 "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [2]+[0]*15, 0xFF)),
    ("Int64x2LessThanSigned",           "Int64x2LessThanSigned",           "Int64x2LtSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [2]+[0]*15, 0xFF)),
    ("Int64x2GreaterThanSigned",        "Int64x2GreaterThanSigned",        "Int64x2GtSMethod",                      "SimdBinaryV128Instruction", "binary",   ([2]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int64x2LessThanOrEqualSigned",    "Int64x2LessThanOrEqualSigned",    "Int64x2LeSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [1]+[0]*15, 0xFF)),
    ("Int64x2GreaterThanOrEqualSigned", "Int64x2GreaterThanOrEqualSigned", "Int64x2GeSMethod",                      "SimdBinaryV128Instruction", "binary",   ([1]+[0]*15, [1]+[0]*15, 0xFF)),

    # float comparisons: equal → 0xFFFFFFFF (4 bytes all set) per lane
    ("Float32x4Equal",                  "Float32x4Equal",                  "Float32x4EqualMethod",                  "SimdBinaryV128Instruction", "binary",   ([0,0,0x80,0x3F]*4, [0,0,0x80,0x3F]*4, 0xFF)),
    ("Float32x4NotEqual",               "Float32x4NotEqual",               "Float32x4NotEqualMethod",               "SimdBinaryV128Instruction", "binary",   ([0,0,0x80,0x3F]*4, [0,0,0x40,0x40]*4, 0xFF)),
    ("Float32x4LessThan",               "Float32x4LessThan",               "Float32x4LtMethod",                     "SimdBinaryV128Instruction", "binary",   ([0,0,0x80,0x3F]*4, [0,0,0x40,0x40]*4, 0xFF)),
    ("Float32x4GreaterThan",            "Float32x4GreaterThan",            "Float32x4GtMethod",                     "SimdBinaryV128Instruction", "binary",   ([0,0,0x40,0x40]*4, [0,0,0x80,0x3F]*4, 0xFF)),
    ("Float32x4LessThanOrEqual",        "Float32x4LessThanOrEqual",        "Float32x4LeMethod",                     "SimdBinaryV128Instruction", "binary",   ([0,0,0x80,0x3F]*4, [0,0,0x80,0x3F]*4, 0xFF)),
    ("Float32x4GreaterThanOrEqual",     "Float32x4GreaterThanOrEqual",     "Float32x4GeMethod",                     "SimdBinaryV128Instruction", "binary",   ([0,0,0x80,0x3F]*4, [0,0,0x80,0x3F]*4, 0xFF)),

    ("Float64x2Equal",                  "Float64x2Equal",                  "Float64x2EqualMethod",                  "SimdBinaryV128Instruction", "binary",   ([0,0,0,0,0,0,0xF0,0x3F]*2, [0,0,0,0,0,0,0xF0,0x3F]*2, 0xFF)),
    ("Float64x2NotEqual",               "Float64x2NotEqual",               "Float64x2NotEqualMethod",               "SimdBinaryV128Instruction", "binary",   ([0,0,0,0,0,0,0xF0,0x3F]*2, [0,0,0,0,0,0,0,0x40]*2, 0xFF)),
    ("Float64x2LessThan",               "Float64x2LessThan",               "Float64x2LtMethod",                     "SimdBinaryV128Instruction", "binary",   ([0,0,0,0,0,0,0xF0,0x3F]*2, [0,0,0,0,0,0,0,0x40]*2, 0xFF)),
    ("Float64x2GreaterThan",            "Float64x2GreaterThan",            "Float64x2GtMethod",                     "SimdBinaryV128Instruction", "binary",   ([0,0,0,0,0,0,0,0x40]*2, [0,0,0,0,0,0,0xF0,0x3F]*2, 0xFF)),
    ("Float64x2LessThanOrEqual",        "Float64x2LessThanOrEqual",        "Float64x2LeMethod",                     "SimdBinaryV128Instruction", "binary",   ([0,0,0,0,0,0,0xF0,0x3F]*2, [0,0,0,0,0,0,0xF0,0x3F]*2, 0xFF)),
    ("Float64x2GreaterThanOrEqual",     "Float64x2GreaterThanOrEqual",     "Float64x2GeMethod",                     "SimdBinaryV128Instruction", "binary",   ([0,0,0,0,0,0,0xF0,0x3F]*2, [0,0,0,0,0,0,0xF0,0x3F]*2, 0xFF)),

    # --- Shifts (v128, i32 → v128) ---
    # ShiftLeft by 1: [1,0,...] → [2,0,...]
    ("Int8x16ShiftLeft",  "Int8x16ShiftLeft",  "Int8x16ShlMethod",  "SimdShiftInstruction", "shift", ([1]+[0]*15, 1, 2)),
    ("Int8x16ShiftRightSigned",   "Int8x16ShiftRightSigned",   "Int8x16ShrSMethod",  "SimdShiftInstruction", "shift", ([0x80]+[0]*15, 1, 0xC0)),
    ("Int8x16ShiftRightUnsigned", "Int8x16ShiftRightUnsigned", "Int8x16ShrUMethod",  "SimdShiftInstruction", "shift", ([0x80]+[0]*15, 1, 0x40)),
    ("Int16x8ShiftLeft",  "Int16x8ShiftLeft",  "Int16x8ShlMethod",  "SimdShiftInstruction", "shift", ([1,0]+[0]*14, 1, 2)),
    ("Int16x8ShiftRightSigned",   "Int16x8ShiftRightSigned",   "Int16x8ShrSMethod",  "SimdShiftInstruction", "shift", ([0,0x80]+[0]*14, 1, 0)),
    ("Int16x8ShiftRightUnsigned", "Int16x8ShiftRightUnsigned", "Int16x8ShrUMethod",  "SimdShiftInstruction", "shift", ([0,0x80]+[0]*14, 1, 0)),
    ("Int32x4ShiftLeft",  "Int32x4ShiftLeft",  "Int32x4ShlMethod",  "SimdShiftInstruction", "shift", ([1,0,0,0]+[0]*12, 1, 2)),
    ("Int32x4ShiftRightSigned",   "Int32x4ShiftRightSigned",   "Int32x4ShrSMethod",  "SimdShiftInstruction", "shift", ([0,0,0,0x80]+[0]*12, 1, 0)),
    ("Int32x4ShiftRightUnsigned", "Int32x4ShiftRightUnsigned", "Int32x4ShrUMethod",  "SimdShiftInstruction", "shift", ([0,0,0,0x80]+[0]*12, 1, 0)),
    ("Int64x2ShiftLeft",  "Int64x2ShiftLeft",  "Int64x2ShlMethod",  "SimdShiftInstruction", "shift", ([1]+[0]*15, 1, 2)),
    ("Int64x2ShiftRightSigned",   "Int64x2ShiftRightSigned",   "Int64x2ShrSMethod",  "SimdShiftInstruction", "shift", ([0]*7+[0x80]+[0]*8, 1, 0)),
    ("Int64x2ShiftRightUnsigned", "Int64x2ShiftRightUnsigned", "Int64x2ShrUMethod",  "SimdShiftInstruction", "shift", ([0]*7+[0x80]+[0]*8, 1, 0)),

    # --- AllTrue/Bitmask/AnyTrue (v128 → i32) ---
    ("Int8x16AllTrue",  "Int8x16AllTrue",  "Int8x16AllTrueMethod",  "SimdV128ToI32Instruction", "v128_i32", ([1]*16, 1)),
    ("Int16x8AllTrue",  "Int16x8AllTrue",  "Int16x8AllTrueMethod",  "SimdV128ToI32Instruction", "v128_i32", ([1,0]*8, 1)),
    ("Int32x4AllTrue",  "Int32x4AllTrue",  "Int32x4AllTrueMethod",  "SimdV128ToI32Instruction", "v128_i32", ([1,0,0,0]*4, 1)),
    ("Int64x2AllTrue",  "Int64x2AllTrue",  "Int64x2AllTrueMethod",  "SimdV128ToI32Instruction", "v128_i32", ([1]+[0]*7+[1]+[0]*7, 1)),
    ("Int8x16Bitmask",  "Int8x16Bitmask",  "Int8x16BitmaskMethod",  "SimdV128ToI32Instruction", "v128_i32", ([0x80]+[0]*15, 1)),
    ("Int16x8Bitmask",  "Int16x8Bitmask",  "Int16x8BitmaskMethod",  "SimdV128ToI32Instruction", "v128_i32", ([0,0x80]+[0]*14, 1)),
    ("Int32x4Bitmask",  "Int32x4Bitmask",  "Int32x4BitmaskMethod",  "SimdV128ToI32Instruction", "v128_i32", ([0,0,0,0x80]+[0]*12, 1)),
    ("Int64x2Bitmask",  "Int64x2Bitmask",  "Int64x2BitmaskMethod",  "SimdV128ToI32Instruction", "v128_i32", ([0]*7+[0x80]+[0]*8, 1)),
    ("V128AnyTrue",     "V128AnyTrue",     "V128AnyTrueMethod",     "SimdV128ToI32Instruction", "v128_i32", ([1]+[0]*15, 1)),

    # --- Unary misc ---
    ("Int8x16Popcnt",   "Int8x16Popcnt",   "Int8x16PopcntMethod",   "SimdUnaryV128Instruction", "unary", ([0xFF]+[0]*15, 8)),
    ("Int8x16AvgrUnsigned", "Int8x16AvgrUnsigned", "Int8x16AvgrUMethod", "SimdBinaryV128Instruction", "binary", ([1]+[0]*15, [3]+[0]*15, 2)),
    ("Int16x8AvgrUnsigned", "Int16x8AvgrUnsigned", "Int16x8AvgrUMethod", "SimdBinaryV128Instruction", "binary", ([1,0]+[0]*14, [3,0]+[0]*14, 2)),

    # --- Narrow (binary v128,v128→v128) ---
    # NarrowSigned: i8x16 from i16x8, lane 0 = 1
    ("Int8x16NarrowInt16x8Signed",   "Int8x16NarrowInt16x8Signed",   "Int8x16NarrowI16x8SMethod",  "SimdBinaryV128Instruction", "binary", ([1,0]+[0]*14, [0]*16, 1)),
    ("Int8x16NarrowInt16x8Unsigned", "Int8x16NarrowInt16x8Unsigned", "Int8x16NarrowI16x8UMethod",  "SimdBinaryV128Instruction", "binary", ([1,0]+[0]*14, [0]*16, 1)),
    ("Int16x8NarrowInt32x4Signed",   "Int16x8NarrowInt32x4Signed",   "Int16x8NarrowI32x4SMethod",  "SimdBinaryV128Instruction", "binary", ([1,0,0,0]+[0]*12, [0]*16, 1)),
    ("Int16x8NarrowInt32x4Unsigned", "Int16x8NarrowInt32x4Unsigned", "Int16x8NarrowI32x4UMethod",  "SimdBinaryV128Instruction", "binary", ([1,0,0,0]+[0]*12, [0]*16, 1)),

    # --- Extend (unary) ---
    ("Int16x8ExtendLowInt8x16Signed",    "Int16x8ExtendLowInt8x16Signed",    "Int16x8ExtLowI8x16SMethod",   "SimdUnaryV128Instruction", "unary", ([1]+[0]*15, 1)),
    ("Int16x8ExtendHighInt8x16Signed",   "Int16x8ExtendHighInt8x16Signed",   "Int16x8ExtHighI8x16SMethod",  "SimdUnaryV128Instruction", "unary", ([0]*8+[2]+[0]*7, 2)),
    ("Int16x8ExtendLowInt8x16Unsigned",  "Int16x8ExtendLowInt8x16Unsigned",  "Int16x8ExtLowI8x16UMethod",   "SimdUnaryV128Instruction", "unary", ([1]+[0]*15, 1)),
    ("Int16x8ExtendHighInt8x16Unsigned", "Int16x8ExtendHighInt8x16Unsigned", "Int16x8ExtHighI8x16UMethod",  "SimdUnaryV128Instruction", "unary", ([0]*8+[2]+[0]*7, 2)),
    ("Int32x4ExtendLowInt16x8Signed",    "Int32x4ExtendLowInt16x8Signed",    "Int32x4ExtLowI16x8SMethod",   "SimdUnaryV128Instruction", "unary", ([1,0]+[0]*14, 1)),
    ("Int32x4ExtendHighInt16x8Signed",   "Int32x4ExtendHighInt16x8Signed",   "Int32x4ExtHighI16x8SMethod",  "SimdUnaryV128Instruction", "unary", ([0]*8+[2,0]+[0]*6, 2)),
    ("Int32x4ExtendLowInt16x8Unsigned",  "Int32x4ExtendLowInt16x8Unsigned",  "Int32x4ExtLowI16x8UMethod",   "SimdUnaryV128Instruction", "unary", ([1,0]+[0]*14, 1)),
    ("Int32x4ExtendHighInt16x8Unsigned", "Int32x4ExtendHighInt16x8Unsigned", "Int32x4ExtHighI16x8UMethod",  "SimdUnaryV128Instruction", "unary", ([0]*8+[2,0]+[0]*6, 2)),
    ("Int64x2ExtendLowInt32x4Signed",    "Int64x2ExtendLowInt32x4Signed",    "Int64x2ExtLowI32x4SMethod",   "SimdUnaryV128Instruction", "unary", ([1,0,0,0]+[0]*12, 1)),
    ("Int64x2ExtendHighInt32x4Signed",   "Int64x2ExtendHighInt32x4Signed",   "Int64x2ExtHighI32x4SMethod",  "SimdUnaryV128Instruction", "unary", ([0]*8+[2,0,0,0]+[0]*4, 2)),
    ("Int64x2ExtendLowInt32x4Unsigned",  "Int64x2ExtendLowInt32x4Unsigned",  "Int64x2ExtLowI32x4UMethod",   "SimdUnaryV128Instruction", "unary", ([1,0,0,0]+[0]*12, 1)),
    ("Int64x2ExtendHighInt32x4Unsigned", "Int64x2ExtendHighInt32x4Unsigned", "Int64x2ExtHighI32x4UMethod",  "SimdUnaryV128Instruction", "unary", ([0]*8+[2,0,0,0]+[0]*4, 2)),

    # --- Extmul (binary) ---
    ("Int16x8ExtmulLowInt8x16Signed",    "Int16x8ExtmulLowInt8x16Signed",    "Int16x8ExtmulLowI8x16SMethod",  "SimdBinaryV128Instruction", "binary", ([2]+[0]*15, [3]+[0]*15, 6)),
    ("Int16x8ExtmulHighInt8x16Signed",   "Int16x8ExtmulHighInt8x16Signed",   "Int16x8ExtmulHighI8x16SMethod", "SimdBinaryV128Instruction", "binary", ([0]*8+[2]+[0]*7, [0]*8+[3]+[0]*7, 6)),
    ("Int16x8ExtmulLowInt8x16Unsigned",  "Int16x8ExtmulLowInt8x16Unsigned",  "Int16x8ExtmulLowI8x16UMethod",  "SimdBinaryV128Instruction", "binary", ([2]+[0]*15, [3]+[0]*15, 6)),
    ("Int16x8ExtmulHighInt8x16Unsigned", "Int16x8ExtmulHighInt8x16Unsigned", "Int16x8ExtmulHighI8x16UMethod", "SimdBinaryV128Instruction", "binary", ([0]*8+[2]+[0]*7, [0]*8+[3]+[0]*7, 6)),
    ("Int32x4ExtmulLowInt16x8Signed",    "Int32x4ExtmulLowInt16x8Signed",    "Int32x4ExtmulLowI16x8SMethod",  "SimdBinaryV128Instruction", "binary", ([2,0]+[0]*14, [3,0]+[0]*14, 6)),
    ("Int32x4ExtmulHighInt16x8Signed",   "Int32x4ExtmulHighInt16x8Signed",   "Int32x4ExtmulHighI16x8SMethod", "SimdBinaryV128Instruction", "binary", ([0]*8+[2,0]+[0]*6, [0]*8+[3,0]+[0]*6, 6)),
    ("Int32x4ExtmulLowInt16x8Unsigned",  "Int32x4ExtmulLowInt16x8Unsigned",  "Int32x4ExtmulLowI16x8UMethod",  "SimdBinaryV128Instruction", "binary", ([2,0]+[0]*14, [3,0]+[0]*14, 6)),
    ("Int32x4ExtmulHighInt16x8Unsigned", "Int32x4ExtmulHighInt16x8Unsigned", "Int32x4ExtmulHighI16x8UMethod", "SimdBinaryV128Instruction", "binary", ([0]*8+[2,0]+[0]*6, [0]*8+[3,0]+[0]*6, 6)),
    ("Int64x2ExtmulLowInt32x4Signed",    "Int64x2ExtmulLowInt32x4Signed",    "Int64x2ExtmulLowI32x4SMethod",  "SimdBinaryV128Instruction", "binary", ([2,0,0,0]+[0]*12, [3,0,0,0]+[0]*12, 6)),
    ("Int64x2ExtmulHighInt32x4Signed",   "Int64x2ExtmulHighInt32x4Signed",   "Int64x2ExtmulHighI32x4SMethod", "SimdBinaryV128Instruction", "binary", ([0]*8+[2,0,0,0]+[0]*4, [0]*8+[3,0,0,0]+[0]*4, 6)),
    ("Int64x2ExtmulLowInt32x4Unsigned",  "Int64x2ExtmulLowInt32x4Unsigned",  "Int64x2ExtmulLowI32x4UMethod",  "SimdBinaryV128Instruction", "binary", ([2,0,0,0]+[0]*12, [3,0,0,0]+[0]*12, 6)),
    ("Int64x2ExtmulHighInt32x4Unsigned", "Int64x2ExtmulHighInt32x4Unsigned", "Int64x2ExtmulHighI32x4UMethod", "SimdBinaryV128Instruction", "binary", ([0]*8+[2,0,0,0]+[0]*4, [0]*8+[3,0,0,0]+[0]*4, 6)),

    # --- Extadd pairwise (unary) ---
    ("Int16x8ExtaddPairwiseInt8x16Signed",    "Int16x8ExtaddPairwiseInt8x16Signed",    "Int16x8ExtaddPairwiseI8x16SMethod",  "SimdUnaryV128Instruction", "unary", ([1,2]+[0]*14, 3)),
    ("Int16x8ExtaddPairwiseInt8x16Unsigned",  "Int16x8ExtaddPairwiseInt8x16Unsigned",  "Int16x8ExtaddPairwiseI8x16UMethod",  "SimdUnaryV128Instruction", "unary", ([1,2]+[0]*14, 3)),
    ("Int32x4ExtaddPairwiseInt16x8Signed",    "Int32x4ExtaddPairwiseInt16x8Signed",    "Int32x4ExtaddPairwiseI16x8SMethod",  "SimdUnaryV128Instruction", "unary", ([1,0,2,0]+[0]*12, 3)),
    ("Int32x4ExtaddPairwiseInt16x8Unsigned",  "Int32x4ExtaddPairwiseInt16x8Unsigned",  "Int32x4ExtaddPairwiseI16x8UMethod",  "SimdUnaryV128Instruction", "unary", ([1,0,2,0]+[0]*12, 3)),

    # --- Q15MulrSat ---
    # Q15: 0x4000 * 0x4000 >> 15 = 0x2000 = [0,0x20,0,0,...]
    ("Int16x8Q15MulrSatSigned", "Int16x8Q15MulrSatSigned", "Int16x8Q15MulrSatSMethod", "SimdBinaryV128Instruction", "binary", ([0,0x40]+[0]*14, [0,0x40]+[0]*14, 0)),

    # --- Dot product ---
    # i32x4.dot_i16x8_s: [1,0,1,0,...] · [2,0,2,0,...] = each pair: 1*2+1*2=4 per i32 lane
    ("Int32x4DotInt16x8Signed", "Int32x4DotInt16x8Signed", "Int32x4DotI16x8SMethod", "SimdBinaryV128Instruction", "binary", ([1,0,1,0]*4, [2,0,2,0]*4, 4)),

    # --- V128Bitselect (ternary: mask, a, b → c) - needs special treatment ---
    # Actually bitselect signature is (v1, v2, mask) - needs a new base class
    # We'll skip it for now and add it specially below.

    # --- Convert/TruncSat/Promote/Demote ---
    # TruncSat: f32x4 → i32x4. 1.0f → 1
    ("Int32x4TruncSatFloat32x4Signed",   "Int32x4TruncSatFloat32x4Signed",   "Int32x4TruncSatF32x4SMethod",  "SimdUnaryV128Instruction", "unary", ([0,0,0x80,0x3F]*4, 1)),
    ("Int32x4TruncSatFloat32x4Unsigned", "Int32x4TruncSatFloat32x4Unsigned", "Int32x4TruncSatF32x4UMethod",  "SimdUnaryV128Instruction", "unary", ([0,0,0x80,0x3F]*4, 1)),
    # TruncSat f64x2 → i32x4 (result zero-padded): 1.0 → [1,0,0,0,0,0,0,0,...] low byte = 1
    ("Int32x4TruncSatFloat64x2SignedZero",   "Int32x4TruncSatFloat64x2SignedZero",   "Int32x4TruncSatF64x2SZeroMethod",  "SimdUnaryV128Instruction", "unary", ([0,0,0,0,0,0,0xF0,0x3F]*2, 1)),
    ("Int32x4TruncSatFloat64x2UnsignedZero", "Int32x4TruncSatFloat64x2UnsignedZero", "Int32x4TruncSatF64x2UZeroMethod",  "SimdUnaryV128Instruction", "unary", ([0,0,0,0,0,0,0xF0,0x3F]*2, 1)),
    # Convert i32x4 → f32x4: [1,0,0,0,...] → 1.0f = [0,0,0x80,0x3F,...]
    ("Float32x4ConvertInt32x4Signed",   "Float32x4ConvertInt32x4Signed",   "Float32x4ConvertI32x4SMethod",  "SimdUnaryV128Instruction", "unary", ([1,0,0,0]*4, 0)),
    ("Float32x4ConvertInt32x4Unsigned", "Float32x4ConvertInt32x4Unsigned", "Float32x4ConvertI32x4UMethod",  "SimdUnaryV128Instruction", "unary", ([1,0,0,0]*4, 0)),
    # Convert i32x4 low → f64x2: [1,0,0,0,...] → 1.0 = [0,0,0,0,0,0,0xF0,0x3F, ...]
    ("Float64x2ConvertLowInt32x4Signed",   "Float64x2ConvertLowInt32x4Signed",   "Float64x2ConvertLowI32x4SMethod",  "SimdUnaryV128Instruction", "unary", ([1,0,0,0]*4, 0)),
    ("Float64x2ConvertLowInt32x4Unsigned", "Float64x2ConvertLowInt32x4Unsigned", "Float64x2ConvertLowI32x4UMethod",  "SimdUnaryV128Instruction", "unary", ([1,0,0,0]*4, 0)),
    # Demote f64x2 → f32x4 (zero-padded): 1.0 = [0,0,0xF0,0x3F,...] → 1.0f = [0,0,0x80,0x3F,...]
    ("Float32x4DemoteFloat64x2Zero", "Float32x4DemoteFloat64x2Zero", "Float32x4DemoteF64x2ZeroMethod", "SimdUnaryV128Instruction", "unary", ([0,0,0,0,0,0,0xF0,0x3F]*2, 0)),
    # Promote f32x4 (low) → f64x2: 1.0f = [0,0,0x80,0x3F,...] → 1.0 = [0,0,0,0,0,0,0xF0,0x3F,...]
    ("Float64x2PromoteLowFloat32x4", "Float64x2PromoteLowFloat32x4", "Float64x2PromoteLowF32x4Method", "SimdUnaryV128Instruction", "unary", ([0,0,0x80,0x3F]*4, 0)),
]

# Generate instruction files
for class_name, simd_op, method_ref, base_class, kind, args in instructions:
    content = make_instr(class_name, simd_op, method_ref, base_class)
    path = os.path.join(INSTR_DEST, f"{class_name}.cs")
    with open(path, "w", newline="\n") as f:
        f.write(content)
    print(f"Created {class_name}.cs")

# Generate test files
for class_name, simd_op, method_ref, base_class, kind, args in instructions:
    if kind == "binary":
        a_bytes, b_bytes, exp0 = args
        a16 = (list(a_bytes)+[0]*16)[:16]
        b16 = (list(b_bytes)+[0]*16)[:16]
        content = make_test_binary(class_name, a16, b16, exp0)
    elif kind == "unary":
        a_bytes, exp0 = args
        a16 = (list(a_bytes)+[0]*16)[:16]
        content = make_test_unary(class_name, a16, exp0)
    elif kind == "v128_i32":
        a_bytes, exp_i32 = args
        a16 = (list(a_bytes)+[0]*16)[:16]
        content = make_test_v128_to_i32(class_name, a16, exp_i32)
    elif kind == "shift":
        a_bytes, shift, exp0 = args
        a16 = (list(a_bytes)+[0]*16)[:16]
        content = make_test_shift(class_name, a16, shift, exp0)
    else:
        continue
    path = os.path.join(TEST_DEST, f"{class_name}Tests.cs")
    with open(path, "w", newline="\n") as f:
        f.write(content)
    print(f"Created {class_name}Tests.cs")

print("Done generating instructions and tests.")
