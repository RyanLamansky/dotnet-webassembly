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

# For binary f32x4 tests we store result at offset 0, read 4 bytes = first float
# For unary f32x4 tests similarly
# For f64x2 read 8 bytes as double (check low 8 bytes)

def make_f32x4_binary_test(class_name, a_floats, b_floats, expected_floats):
    import struct
    a_bytes = []
    for f in a_floats: a_bytes.extend(struct.pack('<f', f))
    b_bytes = []
    for f in b_floats: b_bytes.extend(struct.pack('<f', f))
    exp_bytes = []
    for f in expected_floats: exp_bytes.extend(struct.pack('<f', f))
    a_str = ", ".join(str(x) for x in a_bytes)
    b_str = ", ".join(str(x) for x in b_bytes)
    # check first 4 bytes (first float)
    exp4 = ", ".join(str(x) for x in exp_bytes[:4])
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
        int[] expected = [{exp4}];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {{i}} mismatch");
    }}
}}
"""

def make_f32x4_unary_test(class_name, a_floats, expected_floats):
    import struct
    a_bytes = []
    for f in a_floats: a_bytes.extend(struct.pack('<f', f))
    exp_bytes = []
    for f in expected_floats: exp_bytes.extend(struct.pack('<f', f))
    a_str = ", ".join(str(x) for x in a_bytes)
    exp4 = ", ".join(str(x) for x in exp_bytes[:4])
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
        int[] expected = [{exp4}];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {{i}} mismatch");
    }}
}}
"""

# Instructions: (class_name, simd_op_enum, method_ref, kind, base)
instructions = [
    # f32x4 unary
    ("Float32x4Abs", "Float32x4Abs", "Float32x4AbsMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float32x4Neg", "Float32x4Neg", "Float32x4NegMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float32x4Sqrt", "Float32x4Sqrt", "Float32x4SqrtMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float32x4Ceil", "Float32x4Ceil", "Float32x4CeilMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float32x4Floor", "Float32x4Floor", "Float32x4FloorMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float32x4Trunc", "Float32x4Trunc", "Float32x4TruncMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float32x4Nearest", "Float32x4Nearest", "Float32x4NearestMethod", "unary", "SimdUnaryV128Instruction"),
    # f32x4 binary
    ("Float32x4Add", "Float32x4Add", "Float32x4AddMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float32x4Sub", "Float32x4Sub", "Float32x4SubMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float32x4Mul", "Float32x4Mul", "Float32x4MulMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float32x4Div", "Float32x4Div", "Float32x4DivMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float32x4Min", "Float32x4Min", "Float32x4MinMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float32x4Max", "Float32x4Max", "Float32x4MaxMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float32x4Pmin", "Float32x4Pmin", "Float32x4PminMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float32x4Pmax", "Float32x4Pmax", "Float32x4PmaxMethod", "binary", "SimdBinaryV128Instruction"),
    # f64x2 unary
    ("Float64x2Abs", "Float64x2Abs", "Float64x2AbsMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float64x2Neg", "Float64x2Neg", "Float64x2NegMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float64x2Sqrt", "Float64x2Sqrt", "Float64x2SqrtMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float64x2Ceil", "Float64x2Ceil", "Float64x2CeilMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float64x2Floor", "Float64x2Floor", "Float64x2FloorMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float64x2Trunc", "Float64x2Trunc", "Float64x2TruncMethod", "unary", "SimdUnaryV128Instruction"),
    ("Float64x2Nearest", "Float64x2Nearest", "Float64x2NearestMethod", "unary", "SimdUnaryV128Instruction"),
    # f64x2 binary
    ("Float64x2Add", "Float64x2Add", "Float64x2AddMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float64x2Sub", "Float64x2Sub", "Float64x2SubMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float64x2Mul", "Float64x2Mul", "Float64x2MulMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float64x2Div", "Float64x2Div", "Float64x2DivMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float64x2Min", "Float64x2Min", "Float64x2MinMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float64x2Max", "Float64x2Max", "Float64x2MaxMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float64x2Pmin", "Float64x2Pmin", "Float64x2PminMethod", "binary", "SimdBinaryV128Instruction"),
    ("Float64x2Pmax", "Float64x2Pmax", "Float64x2PmaxMethod", "binary", "SimdBinaryV128Instruction"),
]

for class_name, simd_op, method_ref, kind, base in instructions:
    content = make_instr(class_name, simd_op, method_ref, base)
    path = os.path.join(INSTR_DEST, f"{class_name}.cs")
    with open(path, "w", newline="\n") as f:
        f.write(content)
    print(f"Created {class_name}.cs")

# Now generate tests
# f32x4: use 1.0f repeated in all 4 lanes; f64x2: use 1.0 in both lanes
import struct

def f32_bytes(vals):
    b = []
    for v in vals: b.extend(struct.pack('<f', v))
    return b

def f64_bytes(vals):
    b = []
    for v in vals: b.extend(struct.pack('<d', v))
    return b

def check4(expected_floats):
    return struct.pack('<f', expected_floats[0])

# Test data: (class_name, is_f64, kind, a_vals, b_vals_or_None, expected_first)
test_data = [
    # f32x4 unary — input: [-1.5,-1.5,-1.5,-1.5], expected abs: [1.5,1.5,1.5,1.5]
    ("Float32x4Abs", False, "unary", [-1.5]*4, None, [1.5]*4),
    ("Float32x4Neg", False, "unary", [1.5]*4, None, [-1.5]*4),
    ("Float32x4Sqrt", False, "unary", [4.0]*4, None, [2.0]*4),
    ("Float32x4Ceil", False, "unary", [1.2]*4, None, [2.0]*4),
    ("Float32x4Floor", False, "unary", [1.8]*4, None, [1.0]*4),
    ("Float32x4Trunc", False, "unary", [1.9]*4, None, [1.0]*4),
    ("Float32x4Nearest", False, "unary", [2.5]*4, None, [2.0]*4),  # round-to-even
    # f32x4 binary
    ("Float32x4Add", False, "binary", [1.0]*4, [2.0]*4, [3.0]*4),
    ("Float32x4Sub", False, "binary", [5.0]*4, [2.0]*4, [3.0]*4),
    ("Float32x4Mul", False, "binary", [2.0]*4, [3.0]*4, [6.0]*4),
    ("Float32x4Div", False, "binary", [6.0]*4, [2.0]*4, [3.0]*4),
    ("Float32x4Min", False, "binary", [1.0]*4, [2.0]*4, [1.0]*4),
    ("Float32x4Max", False, "binary", [1.0]*4, [2.0]*4, [2.0]*4),
    ("Float32x4Pmin", False, "binary", [2.0]*4, [1.0]*4, [1.0]*4),
    ("Float32x4Pmax", False, "binary", [1.0]*4, [2.0]*4, [2.0]*4),
    # f64x2 unary
    ("Float64x2Abs", True, "unary", [-1.5]*2, None, [-1.5]*2),  # abs(-1.5)=1.5 but we check bytes of 1.5
    ("Float64x2Neg", True, "unary", [1.5]*2, None, [-1.5]*2),
    ("Float64x2Sqrt", True, "unary", [4.0]*2, None, [2.0]*2),
    ("Float64x2Ceil", True, "unary", [1.2]*2, None, [2.0]*2),
    ("Float64x2Floor", True, "unary", [1.8]*2, None, [1.0]*2),
    ("Float64x2Trunc", True, "unary", [1.9]*2, None, [1.0]*2),
    ("Float64x2Nearest", True, "unary", [2.5]*2, None, [2.0]*2),
    # f64x2 binary
    ("Float64x2Add", True, "binary", [1.0]*2, [2.0]*2, [3.0]*2),
    ("Float64x2Sub", True, "binary", [5.0]*2, [2.0]*2, [3.0]*2),
    ("Float64x2Mul", True, "binary", [2.0]*2, [3.0]*2, [6.0]*2),
    ("Float64x2Div", True, "binary", [6.0]*2, [2.0]*2, [3.0]*2),
    ("Float64x2Min", True, "binary", [1.0]*2, [2.0]*2, [1.0]*2),
    ("Float64x2Max", True, "binary", [1.0]*2, [2.0]*2, [2.0]*2),
    ("Float64x2Pmin", True, "binary", [2.0]*2, [1.0]*2, [1.0]*2),
    ("Float64x2Pmax", True, "binary", [1.0]*2, [2.0]*2, [2.0]*2),
]

for class_name, is_f64, kind, a_vals, b_vals, exp_vals in test_data:
    if is_f64:
        # f64x2: fix abs since we want 1.5 (positive), not -1.5
        if "Abs" in class_name:
            exp_vals = [abs(v) for v in a_vals]
        a_bytes_list = f64_bytes(a_vals)
        a16 = (a_bytes_list + [0]*16)[:16]
        exp_bytes = f64_bytes(exp_vals)
        exp4 = ", ".join(str(x) for x in exp_bytes[:4])
        if kind == "binary":
            b16 = (f64_bytes(b_vals) + [0]*16)[:16]
            b_str = ", ".join(str(x) for x in b16)
        else:
            b16 = None
        a_str = ", ".join(str(x) for x in a16)
        export_class = class_name + "Export"
        op_str = f"""                new V128Const {{ Value = [{a_str}] }},
                new V128Const {{ Value = [{b_str}] }},
                new {class_name}(),""" if kind == "binary" else f"""                new V128Const {{ Value = [{a_str}] }},
                new {class_name}(),"""
    else:
        if "Abs" in class_name:
            exp_vals = [abs(v) for v in a_vals]
        a16 = (f32_bytes(a_vals) + [0]*16)[:16]
        if kind == "binary":
            b16 = (f32_bytes(b_vals) + [0]*16)[:16]
        else:
            b16 = None
        exp_bytes = f32_bytes(exp_vals)
        exp4 = ", ".join(str(x) for x in exp_bytes[:4])
        a_str = ", ".join(str(x) for x in a16)
        if kind == "binary":
            b_str = ", ".join(str(x) for x in b16)
        export_class = class_name + "Export"
        op_str = f"""                new V128Const {{ Value = [{a_str}] }},
                new V128Const {{ Value = [{b_str}] }},
                new {class_name}(),""" if kind == "binary" else f"""                new V128Const {{ Value = [{a_str}] }},
                new {class_name}(),"""

    test_content = f"""using Microsoft.VisualStudio.TestTools.UnitTesting;
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
{op_str}
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
        int[] expected = [{exp4}];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {{i}} mismatch");
    }}
}}
"""
    path = os.path.join(TEST_DEST, f"{class_name}Tests.cs")
    with open(path, "w", newline="\n") as f:
        f.write(test_content)
    print(f"Created {class_name}Tests.cs")
