using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace WebAssembly.Runtime;

/// <summary>
/// Runs the official specification's tests.
/// </summary>
[TestClass]
public class SpecTests
{
    private static string DataPath(params string[] parts)
        => Path.Combine(["Runtime", "SpecTestData", ..parts]);

    // ==== WASM 1.0 ====

    /// <summary>Runs the address tests.</summary>
    [TestMethod]
    public void SpecTest_address()
    {
        // 1x AssertFailedException: offset out of range doesn't have a test procedure set up.
        var skips = new HashSet<uint>
        {
            214
        };
        SpecTestRunner.Run(DataPath("address"), "address.json", skips.Contains);
    }

    /// <summary>Runs the align tests.</summary>
    [TestMethod]
    public void SpecTest_align()
    {
        // 37x AssertFailedException: expected CompilerException not thrown
        var skips = new HashSet<uint>
        {
            306, 310, 314, 318, 322, 326, 330, 334, 338, 342, 346, 350, 354, 358, 363, 367, 371, 375, 379,
            383, 387, 391, 395, 399, 403, 407, 411, 415, 420, 424, 428, 432, 436, 440, 444, 448, 452
        };
        SpecTestRunner.Run(DataPath("align"), "align.json", skips.Contains);
    }

    /// <summary>Runs the binary leb128 tests.</summary>
    [TestMethod]
    public void SpecTest_binary_leb128()
    {
        // 4x ModuleLoadException: At offset N: At offset N: Opcode "Int64DivideUnsigned" is not permitted in intializer expressions.
        // 1x StackTypeInvalidException: Int32Load requires the top stack item to be Int32, found Int64.
        // 1x ModuleLoadException: At offset N: At offset N: Don't know how to parse miscellaneous opcode "N".
        // 2x ModuleLoadException: At offset N: At offset N: Opcode "Unreachable" is not permitted in intializer expressions.
        var skips = new HashSet<uint>
        {
            32, 881, 998, 1044, 1053, 1072, 1081, 1090
        };
        SpecTestRunner.Run(DataPath("binary-leb128"), "binary-leb128.json", skips.Contains);
    }

    /// <summary>Runs the binary tests.</summary>
    [TestMethod]
    public void SpecTest_binary()
    {
        // 2x ModuleLoadException: At offset N: At offset N: Opcode "Unreachable" is not permitted in intializer expressions.
        // 3x ModuleLoadException: At offset N: At offset N: Opcode "Int64DivideUnsigned" is not permitted in intializer expressions.
        // 1x ModuleLoadException: At offset N: Operation is not valid due to the current state of the object.
        // 1x ModuleLoadException: At offset N: Code section found but functionSignatures is null
        // 2x ModuleLoadException: At offset N: At offset N: Opcode "Int32RemainderUnsigned" is not permitted in intializer expressions.
        // 1x ModuleLoadException: At offset N: Stream ended unexpectedly.
        var skips = new HashSet<uint>
        {
            152, 161, 180, 189, 198, 1172, 1178, 1297, 1321, 1538
        };
        SpecTestRunner.Run(DataPath("binary"), "binary.json", skips.Contains);
    }

    /// <summary>Runs the block tests.</summary>
    [TestMethod]
    public void SpecTest_block()
    {
        SpecTestRunner.Run(DataPath("block"), "block.json");
    }

    /// <summary>Runs the br tests.</summary>
    [TestMethod]
    public void SpecTest_br()
    {
        SpecTestRunner.Run(DataPath("br"), "br.json");
    }

    /// <summary>Runs the br_if tests.</summary>
    [TestMethod]
    public void SpecTest_br_if()
    {
        SpecTestRunner.Run(DataPath("br_if"), "br_if.json");
    }

    /// <summary>Runs the br_table tests.</summary>
    [TestMethod]
    public void SpecTest_br_table()
    {
        SpecTestRunner.Run(DataPath("br_table"), "br_table.json");
    }

    /// <summary>Runs the call tests.</summary>
    [TestMethod]
    public void SpecTest_call()
    {
        SpecTestRunner.Run(DataPath("call"), "call.json");
    }

    /// <summary>Runs the call_indirect tests.</summary>
    [TestMethod]
    public void SpecTest_call_indirect()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Unrecognized section type N.
        // 114x AssertFailedException: no method source (cascaded module-load failure)
        // 11x AssertFailedException: got AssertFailedException, expected InvalidCastException
        // 6x AssertFailedException: Expected ModuleLoadException or IndexOutOfRangeException, but received AssertFailedException.
        // 2x AssertFailedException: got AssertFailedException, expected StackOverflowException
        // 1x ModuleLoadException: At offset N: Stream ended unexpectedly.
        // 1x AssertFailedException: Expected KeyNotFoundException or NullReferenceException, but received AssertFailedException.
        var skips = new HashSet<uint>
        {
            3, 471, 472, 473, 474, 475, 477, 479, 480, 481, 482, 484, 485, 486, 487, 489, 490, 491, 493,
            494, 495, 496, 497, 498, 499, 500, 501, 502, 504, 505, 506, 507, 508, 509, 511, 512, 513, 514,
            515, 516, 518, 519, 520, 521, 522, 523, 525, 526, 527, 528, 529, 530, 532, 533, 534, 535, 537,
            538, 539, 540, 542, 543, 544, 545, 547, 548, 549, 550, 552, 553, 554, 555, 556, 558, 559, 560,
            561, 562, 564, 565, 566, 567, 568, 570, 571, 572, 573, 574, 576, 577, 578, 579, 580, 581, 582,
            583, 585, 586, 588, 589, 590, 592, 594, 595, 597, 598, 600, 601, 603, 604, 605, 606, 607, 608,
            609, 610, 612, 613, 614, 615, 616, 617, 618, 623, 650, 651, 652, 654, 655, 656, 657, 659, 660,
            661, 662, 663
        };
        SpecTestRunner.Run(DataPath("call_indirect"), "call_indirect.json", skips.Contains);
    }

    /// <summary>Runs the comments tests.</summary>
    [TestMethod]
    public void SpecTest_comments() => SpecTestRunner.Run(DataPath("comments"), "comments.json");

    /// <summary>Runs the const tests.</summary>
    [TestMethod]
    public void SpecTest_const() => SpecTestRunner.Run(DataPath("const"), "const.json");

    /// <summary>Runs the conversions tests.</summary>
    [TestMethod]
    public void SpecTest_conversions()
    {
        // 8x AssertFailedException: Arithmetic operation overflow
        // 4x AssertFailedException: expected OverflowException not thrown
        var skips = new HashSet<uint>
        {
            96, 97, 101, 143, 144, 149, 151, 195, 199, 241, 246, 248
        };
        SpecTestRunner.Run(DataPath("conversions"), "conversions.json", skips.Contains);
    }

    /// <summary>Runs the custom tests.</summary>
    [TestMethod]
    public void SpecTest_custom()
    {
        // 1x ModuleLoadException: At offset N: WebAssemblyValueType N not recognized. (Parameter 'valueType')
        var skips = new HashSet<uint>
        {
            14
        };
        SpecTestRunner.Run(DataPath("custom"), "custom.json", skips.Contains);
    }

    /// <summary>Runs the data tests.</summary>
    [TestMethod]
    public void SpecTest_data()
    {
        // 4x ModuleLoadException: At offset N: At offset N: Imported external kind of N is not recognized.
        // 5x AssertFailedException: expected exception not thrown
        // 3x MemoryAccessOutOfRangeException: MemoryAccessOutOfRange
        // 14x AssertFailedException: out of bounds memory access doesn't have a test procedure set up.
        // 4x AssertFailedException: unknown memory doesn't have a test procedure set up.
        // 2x AssertFailedException: expected ModuleLoadException not thrown
        // 2x AssertFailedException: unknown global doesn't have a test procedure set up.
        var skips = new HashSet<uint>
        {
            67, 78, 85, 89, 96, 101, 107, 155, 161, 180, 188, 196, 203, 210, 227, 236, 243, 251, 259, 267,
            274, 282, 289, 307, 331, 343, 365, 408, 416, 425, 466, 475, 483, 492
        };
        SpecTestRunner.Run(DataPath("data"), "data.json", skips.Contains);
    }

    /// <summary>Runs the elem tests.</summary>
    [TestMethod]
    public void SpecTest_elem()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "Int32RemainderUnsigned" is not permitted in intializer expressions.
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "7" is not permitted in intializer expressions.
        // 2x ModuleLoadException: At offset N: Initializer expression support for the Element section is limited to a single Int32 constant followed by end.
        // 1x ModuleLoadException: At offset N: At offset N: Unrecognized section type N.
        // 1x ModuleLoadException: At offset N: At offset N: Don't know how to parse miscellaneous opcode "N".
        // 2x ModuleLoadException: At offset N: At offset N: Opcode "Unreachable" is not permitted in intializer expressions.
        // 1x ArgumentException: The delegate at position 9 is expected to be of type System.Action, but the supplied delegate is System.Func`1[System.Int32]. (Parameter 'value')
        // 2x AssertFailedException: method-lookup failure
        // 2x ImportException: Missing import for module1::shared-table.
        // 2x AssertFailedException: Object reference not set to an instance of an object.
        // 3x AssertFailedException: Not equal iN: A and B (NaN payload mismatch)
        // 1x ModuleLoadException: At offset N: At offset N: Don't know how to parse opcode "N".
        // 10x KeyNotFoundException: The given key '$X' was not present in the dictionary.
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "210" is not permitted in intializer expressions.
        // 1x ModuleLoadException: At offset N: WebAssemblyValueType N not recognized. (Parameter 'valueType')
        var skips = new HashSet<uint>
        {
            4, 80, 120, 127, 153, 550, 561, 589, 598, 599, 602, 611, 612, 613
        };
        SpecTestRunner.Run(DataPath("elem"), "elem.json", skips.Contains);
    }

    /// <summary>Runs the endianness tests.</summary>
    [TestMethod]
    public void SpecTest_endianness() => SpecTestRunner.Run(DataPath("endianness"), "endianness.json");

    /// <summary>Runs the exports tests.</summary>
    [TestMethod]
    public void SpecTest_exports()
    {
        // 18x AssertFailedException: expected ModuleLoadException not thrown
        // 1x ModuleLoadException: At offset N: Overflow encountered.
        var skips = new HashSet<uint>
        {
            51, 55, 59, 63, 67, 108, 112, 116, 120, 124, 133, 163, 171, 175, 179, 219, 228, 232, 236
        };
        SpecTestRunner.Run(DataPath("exports"), "exports.json", skips.Contains);
    }

    /// <summary>Runs the f32 tests.</summary>
    [TestMethod]
    public void SpecTest_f32() => SpecTestRunner.Run(DataPath("f32"), "f32.json");

    /// <summary>Runs the f32_bitwise tests.</summary>
    [TestMethod]
    public void SpecTest_f32_bitwise() => SpecTestRunner.Run(DataPath("f32_bitwise"), "f32_bitwise.json");

    /// <summary>Runs the f32_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_f32_cmp() => SpecTestRunner.Run(DataPath("f32_cmp"), "f32_cmp.json");

    /// <summary>Runs the f64 tests.</summary>
    [TestMethod]
    public void SpecTest_f64() => SpecTestRunner.Run(DataPath("f64"), "f64.json");

    /// <summary>Runs the f64_bitwise tests.</summary>
    [TestMethod]
    public void SpecTest_f64_bitwise() => SpecTestRunner.Run(DataPath("f64_bitwise"), "f64_bitwise.json");

    /// <summary>Runs the f64_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_f64_cmp() => SpecTestRunner.Run(DataPath("f64_cmp"), "f64_cmp.json");

    /// <summary>Runs the fac tests.</summary>
    [TestMethod]
    public void SpecTest_fac()
    {
        // 1x ModuleLoadException: At offset N: WebAssemblyValueType N not recognized. (Parameter 'valueType')
        // 6x AssertFailedException: no method source (cascaded module-load failure)
        // 1x AssertFailedException: got AssertFailedException, expected StackOverflowException
        var skips = new HashSet<uint>
        {
            1, 102, 103, 104, 105, 106, 107, 109
        };
        SpecTestRunner.Run(DataPath("fac"), "fac.json", skips.Contains);
    }

    /// <summary>Runs the float_exprs tests.</summary>
    [TestMethod]
    public void SpecTest_float_exprs()
    {
        // 2x AssertFailedException: Arithmetic operation overflow
        // 7x AssertFailedException: Not equal iN: A and B (NaN payload mismatch)
        var skips = new HashSet<uint>
        {
            511, 519, 2349, 2351, 2353, 2355, 2357, 2359, 2361
        };
        SpecTestRunner.Run(DataPath("float_exprs"), "float_exprs.json", skips.Contains);
    }

    /// <summary>Runs the float_literals tests.</summary>
    [TestMethod]
    public void SpecTest_float_literals()
    {
        // 4x AssertFailedException: Not equal iN: A and B (NaN payload mismatch)
        var skips = new HashSet<uint>
        {
            125, 127, 128, 129
        };
        SpecTestRunner.Run(DataPath("float_literals"), "float_literals.json", skips.Contains);
    }

    /// <summary>Runs the float_memory tests.</summary>
    [TestMethod]
    public void SpecTest_float_memory()
    {
        // 2x AssertFailedException: Not equal iN: A and B (NaN payload mismatch)
        var skips = new HashSet<uint>
        {
            21, 73
        };
        SpecTestRunner.Run(DataPath("float_memory"), "float_memory.json", skips.Contains);
    }

    /// <summary>Runs the float_misc tests.</summary>
    [TestMethod]
    public void SpecTest_float_misc() => SpecTestRunner.Run(DataPath("float_misc"), "float_misc.json");

    /// <summary>Runs the forward tests.</summary>
    [TestMethod]
    public void SpecTest_forward() => SpecTestRunner.Run(DataPath("forward"), "forward.json");

    /// <summary>Runs the func tests.</summary>
    [TestMethod]
    public void SpecTest_func()
    {
        // 1x ModuleLoadException: At offset N: WebAssemblyValueType N not recognized. (Parameter 'valueType')
        // 89x AssertFailedException: no method source (cascaded module-load failure)
        // 1x ModuleLoadException: At offset N: Stream ended unexpectedly.
        // 4x AssertFailedException: method-lookup failure
        var skips = new HashSet<uint>
        {
            3, 241, 242, 243, 245, 248, 249, 251, 255, 256, 257, 258, 259, 260, 261, 262, 263, 266, 269,
            272, 275, 278, 281, 284, 287, 291, 298, 299, 300, 301, 302, 303, 304, 305, 308, 309, 310, 312,
            313, 314, 315, 316, 317, 318, 321, 322, 324, 325, 326, 327, 328, 329, 330, 333, 334, 336, 337,
            338, 339, 340, 343, 347, 348, 349, 350, 351, 352, 353, 354, 355, 358, 361, 364, 367, 368, 369,
            370, 372, 375, 378, 381, 384, 388, 392, 396, 401, 414, 415, 416, 417, 488, 551, 552, 553, 554,
            828
        };
        SpecTestRunner.Run(DataPath("func"), "func.json", skips.Contains);
    }

    /// <summary>Runs the func_ptrs tests.</summary>
    [TestMethod]
    public void SpecTest_func_ptrs()
    {
        // 1x ModuleLoadException: At offset N: Stream ended unexpectedly.
        // 16x AssertFailedException: method-lookup failure
        // 6x AssertFailedException: Expected ModuleLoadException or IndexOutOfRangeException, but received AssertFailedException.
        // 1x ModuleLoadException: At offset N: At offset N: Unrecognized section type N.
        var skips = new HashSet<uint>
        {
            51, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 93, 105,
            106
        };
        SpecTestRunner.Run(DataPath("func_ptrs"), "func_ptrs.json", skips.Contains);
    }

    /// <summary>Runs the global tests.</summary>
    [TestMethod]
    public void SpecTest_global()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Imported external kind of N is not recognized.
        // 57x AssertFailedException: no method source (cascaded module-load failure)
        // 1x AssertFailedException: Expected ModuleLoadException or IndexOutOfRangeException, but received AssertFailedException.
        // 2x AssertFailedException: expected exception not thrown
        // 1x AssertFailedException: expected ModuleLoadException not thrown
        var skips = new HashSet<uint>
        {
            3, 196, 197, 198, 199, 200, 201, 202, 203, 205, 206, 207, 208, 210, 211, 213, 214, 216, 217,
            218, 219, 221, 222, 223, 225, 226, 227, 228, 229, 231, 232, 233, 235, 236, 237, 239, 240, 241,
            243, 244, 246, 247, 249, 250, 251, 253, 254, 255, 256, 258, 260, 261, 262, 264, 265, 266, 268,
            269, 270, 352, 356, 371
        };
        SpecTestRunner.Run(DataPath("global"), "global.json", skips.Contains);
    }

    /// <summary>Runs the i32 tests.</summary>
    [TestMethod]
    public void SpecTest_i32()
    {
        // 1x AssertFailedException: Arithmetic operation overflow
        var skips = new HashSet<uint>
        {
            109
        };
        SpecTestRunner.Run(DataPath("i32"), "i32.json", skips.Contains);
    }

    /// <summary>Runs the i64 tests.</summary>
    [TestMethod]
    public void SpecTest_i64()
    {
        // 1x AssertFailedException: Arithmetic operation overflow
        var skips = new HashSet<uint>
        {
            110
        };
        SpecTestRunner.Run(DataPath("i64"), "i64.json", skips.Contains);
    }

    /// <summary>Runs the if tests.</summary>
    [TestMethod]
    public void SpecTest_if()
    {
        SpecTestRunner.Run(DataPath("if"), "if.json");
    }

    /// <summary>Runs the imports tests.</summary>
    [TestMethod]
    public void SpecTest_imports()
    {
        // 1x ModuleLoadException: At offset N: Sections out of order; section Type encountered after Table.
        // 1x AssertFailedException: Assert.IsNotNull failed. 21 tried to register null as a module method source.
        // 3x ModuleLoadException: At offset N: At offset N: Imported external kind of N is not recognized.
        // 2x AssertFailedException: no method source (cascaded module-load failure)
        // 1x ImportException: Missing import for test::func.
        // 1x ImportException: Missing import for test::func-i32.
        // 1x ImportException: Missing import for test::func-f32.
        // 1x ImportException: Missing import for test::func->i32.
        // 1x ImportException: Missing import for test::func->f32.
        // 1x ImportException: Missing import for test::func-i32->i32.
        // 1x ImportException: Missing import for test::func-i64->i64.
        // 7x AssertFailedException: method-lookup failure
        // 1x ImportException: Missing import for test::global-i32.
        // 1x ImportException: Missing import for test::global-f32.
        // 1x ImportException: Missing import for test::global-mut-i64.
        // 3x ImportException: Missing import for test::table-10-inf.
        // 9x ImportException: Missing import for test::table-10-N.
        // 6x AssertFailedException: expected ImportException not thrown
        // 3x ImportException: Missing import for test::memory-2-inf.
        // 1x ImportException: Missing import for grown-memory::memory.
        // 3x KeyNotFoundException: The given key '$X' was not present in the dictionary.
        // 1x ImportException: Missing import for grown-imported-memory::memory.
        // 1x AssertFailedException: got ModuleLoadException, expected ImportException
        var skips = new HashSet<uint>
        {
            3, 21, 26, 85, 86, 116, 117, 118, 119, 120, 121, 122, 226, 246, 247, 248, 249, 250, 251, 252,
            254, 255, 256, 382, 389, 390, 391, 392, 393, 394, 395, 396, 397, 398, 399, 400, 436, 440, 501,
            502, 503, 529, 533, 563, 567, 587, 592, 593, 594, 599, 678
        };
        SpecTestRunner.Run(DataPath("imports"), "imports.json", skips.Contains);
    }

    /// <summary>Runs the inline module tests.</summary>
    [TestMethod]
    public void SpecTest_inline_module() => SpecTestRunner.Run(DataPath("inline-module"), "inline-module.json");

    /// <summary>Runs the int_exprs tests.</summary>
    [TestMethod]
    public void SpecTest_int_exprs() => SpecTestRunner.Run(DataPath("int_exprs"), "int_exprs.json");

    /// <summary>Runs the int_literals tests.</summary>
    [TestMethod]
    public void SpecTest_int_literals() => SpecTestRunner.Run(DataPath("int_literals"), "int_literals.json");

    /// <summary>Runs the labels tests.</summary>
    [TestMethod]
    [Ignore("StackTooSmallException")]
    public void SpecTest_labels() => SpecTestRunner.Run(DataPath("labels"), "labels.json");

    /// <summary>Runs the left to right tests.</summary>
    [TestMethod]
    public void SpecTest_left_to_right()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Unrecognized section type N.
        // 95x AssertFailedException: no method source (cascaded module-load failure)
        var skips = new HashSet<uint>
        {
            1, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198,
            199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 213, 214, 215, 216, 217, 218,
            219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 232, 233
        };
        SpecTestRunner.Run(DataPath("left-to-right"), "left-to-right.json", skips.Contains);
    }

    /// <summary>Runs the linking tests.</summary>
    [TestMethod]
    public void SpecTest_linking()
    {
        // 1x ArgumentException: setter cannot have a return type. (Parameter 'setter')
        // 5x ModuleLoadException: At offset N: At offset N: Imported external kind of N is not recognized.
        // 35x KeyNotFoundException: The given key '$X' was not present in the dictionary.
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "208" is not permitted in intializer expressions.
        // 2x KeyNotFoundException: The given key '$X_ex' was not present in the dictionary.
        // 7x AssertFailedException: got ModuleLoadException, expected ImportException
        // 4x AssertFailedException: Expected ModuleLoadException or IndexOutOfRangeException, but received KeyNotFoundException.
        // 1x AssertFailedException: got KeyNotFoundException, expected InvalidCastException
        // 6x AssertFailedException: Not equal iN: A and B (NaN payload mismatch)
        // 4x AssertFailedException: Object reference not set to an instance of an object.
        // 1x ImportException: Missing import for Mt::tab.
        // 1x AssertFailedException: Common Language Runtime detected an invalid program.
        // 3x AssertFailedException: out of bounds table access doesn't have a test procedure set up.
        // 3x AssertFailedException: out of bounds memory access doesn't have a test procedure set up.
        // 1x ModuleLoadException: At offset N: The only supported table element type is FunctionReference, found -17
        // 3x ImportException: Missing import for Mm::mem.
        // 1x AssertFailedException: MemoryAccessOutOfRange
        var skips = new HashSet<uint>
        {
            48, 50, 68, 69, 71, 72, 75, 77, 81, 83, 96, 102, 104, 113, 117, 123, 127, 149, 169, 170, 171,
            175, 180, 184, 185, 186, 188, 189, 191, 205, 206, 207, 209, 210, 211, 212, 213, 215, 216, 217,
            218, 219, 223, 227, 229, 241, 244, 253, 267, 275, 279, 288, 291, 295, 297, 303, 340, 349, 350,
            352, 354, 360, 367, 375, 376, 377, 378, 379, 380, 381, 382, 385, 398, 406, 407, 410, 419, 452,
            453
        };
        SpecTestRunner.Run(DataPath("linking"), "linking.json", skips.Contains);
    }

    /// <summary>Runs the load tests.</summary>
    [TestMethod]
    public void SpecTest_load()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "Branch" is not permitted in intializer expressions.
        // 37x AssertFailedException: no method source (cascaded module-load failure)
        var skips = new HashSet<uint>
        {
            3, 161, 163, 164, 165, 167, 168, 169, 171, 173, 174, 175, 177, 178, 179, 181, 182, 183, 185,
            186, 187, 188, 190, 191, 192, 194, 195, 196, 197, 198, 199, 201, 203, 204, 206, 208, 209, 211
        };
        SpecTestRunner.Run(DataPath("load"), "load.json", skips.Contains);
    }

    /// <summary>Runs the local_get tests.</summary>
    [TestMethod]
    public void SpecTest_local_get() => SpecTestRunner.Run(DataPath("local_get"), "local_get.json");

    /// <summary>Runs the local_set tests.</summary>
    [TestMethod]
    public void SpecTest_local_set() => SpecTestRunner.Run(DataPath("local_set"), "local_set.json");

    /// <summary>Runs the local_tee tests.</summary>
    [TestMethod]
    public void SpecTest_local_tee()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "8" is not permitted in intializer expressions.
        // 55x AssertFailedException: no method source (cascaded module-load failure)
        var skips = new HashSet<uint>
        {
            3, 280, 281, 282, 283, 285, 286, 287, 288, 290, 291, 292, 294, 295, 296, 298, 300, 301, 302,
            304, 305, 306, 308, 310, 311, 312, 314, 315, 316, 318, 319, 320, 322, 323, 324, 325, 327, 328,
            329, 331, 332, 333, 334, 335, 336, 338, 339, 340, 341, 342, 343, 344, 345, 348, 354, 361
        };
        SpecTestRunner.Run(DataPath("local_tee"), "local_tee.json", skips.Contains);
    }

    /// <summary>Runs the loop tests.</summary>
    [TestMethod]
    public void SpecTest_loop()
    {
        SpecTestRunner.Run(DataPath("loop"), "loop.json");
    }

    /// <summary>Runs the memory tests.</summary>
    [TestMethod]
    public void SpecTest_memory()
    {
        // 7x AssertFailedException: expected ModuleLoadException not thrown
        // 3x AssertFailedException: got FileNotFoundException, expected ModuleLoadException
        var skips = new HashSet<uint>
        {
            51, 55, 59, 63, 67, 71, 75, 80, 84, 88
        };
        SpecTestRunner.Run(DataPath("memory"), "memory.json", skips.Contains);
    }

    /// <summary>Runs the memory_grow tests.</summary>
    [TestMethod]
    public void SpecTest_memory_grow()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "Int64DivideSigned" is not permitted in intializer expressions.
        // 36x AssertFailedException: method-lookup failure
        // 1x AssertFailedException: Expected ModuleLoadException or IndexOutOfRangeException, but received AssertFailedException.
        var skips = new HashSet<uint>
        {
            101, 259, 261, 262, 263, 265, 266, 267, 269, 271, 272, 273, 275, 276, 277, 279, 280, 281, 283,
            284, 285, 286, 288, 289, 290, 292, 293, 294, 295, 296, 297, 299, 301, 302, 304, 306, 307, 309
        };
        SpecTestRunner.Run(DataPath("memory_grow"), "memory_grow.json", skips.Contains);
    }

    /// <summary>Runs the memory_redundancy tests.</summary>
    [TestMethod]
    public void SpecTest_memory_redundancy() => SpecTestRunner.Run(DataPath("memory_redundancy"), "memory_redundancy.json");

    /// <summary>Runs the memory_size tests.</summary>
    [TestMethod]
    public void SpecTest_memory_size() => SpecTestRunner.Run(DataPath("memory_size"), "memory_size.json");

    /// <summary>Runs the memory_trap tests.</summary>
    [TestMethod]
    public void SpecTest_memory_trap()
    {
        // 1x MemoryAccessOutOfRangeException: MemoryAccessOutOfRange
        // 160x AssertFailedException: unexpected AssertFailedException
        // 7x AssertFailedException: method-lookup failure
        var skips = new HashSet<uint>
        {
            35, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128,
            129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147,
            148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166,
            167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185,
            186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204,
            205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223,
            224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242,
            243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261,
            262, 263, 264, 265, 266, 269, 270, 274, 275, 276, 277, 278, 279, 280, 281, 282
        };
        SpecTestRunner.Run(DataPath("memory_trap"), "memory_trap.json", skips.Contains);
    }

    /// <summary>Runs the names tests.</summary>
    [TestMethod]
    public void SpecTest_names()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Imported external kind of N is not recognized.
        // 1x AssertFailedException: method-lookup failure
        var skips = new HashSet<uint>
        {
            1095, 1107
        };
        SpecTestRunner.Run(DataPath("names"), "names.json", skips.Contains);
    }

    /// <summary>Runs the nop tests.</summary>
    [TestMethod]
    public void SpecTest_nop()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "Block" is not permitted in intializer expressions.
        // 83x AssertFailedException: no method source (cascaded module-load failure)
        var skips = new HashSet<uint>
        {
            3, 306, 307, 308, 309, 311, 312, 313, 315, 316, 317, 318, 319, 321, 322, 323, 324, 326, 327,
            328, 329, 331, 332, 333, 334, 335, 336, 338, 339, 340, 342, 343, 344, 345, 347, 348, 349, 350,
            352, 353, 354, 356, 357, 358, 359, 360, 362, 363, 364, 366, 367, 368, 369, 371, 372, 373, 375,
            376, 377, 378, 380, 381, 382, 384, 385, 386, 387, 388, 390, 391, 392, 394, 395, 396, 398, 399,
            400, 402, 403, 404, 406, 407, 408, 409
        };
        SpecTestRunner.Run(DataPath("nop"), "nop.json", skips.Contains);
    }

    /// <summary>Runs the obsolete keywords tests.</summary>
    [TestMethod]
    public void SpecTest_obsolete_keywords() => SpecTestRunner.Run(DataPath("obsolete-keywords"), "obsolete-keywords.json");

    /// <summary>Runs the return tests.</summary>
    [TestMethod]
    public void SpecTest_return()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "Block" is not permitted in intializer expressions.
        // 63x AssertFailedException: no method source (cascaded module-load failure)
        var skips = new HashSet<uint>
        {
            3, 224, 225, 226, 227, 229, 230, 231, 232, 234, 235, 237, 238, 239, 240, 242, 243, 244, 245,
            247, 248, 249, 251, 253, 254, 255, 257, 258, 259, 261, 263, 264, 265, 266, 267, 269, 270, 271,
            272, 273, 275, 276, 277, 279, 280, 281, 282, 284, 285, 286, 288, 289, 291, 292, 293, 294, 296,
            298, 299, 301, 303, 304, 306, 308
        };
        SpecTestRunner.Run(DataPath("return"), "return.json", skips.Contains);
    }

    /// <summary>Runs the select tests.</summary>
    [TestMethod]
    public void SpecTest_select()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "6" is not permitted in intializer expressions.
        // 116x AssertFailedException: no method source (cascaded module-load failure)
        // 2x AssertFailedException: Expected ModuleLoadException or IndexOutOfRangeException, but received AssertFailedException.
        // 1x AssertFailedException: got StackTooSmallException, expected ModuleLoadException
        // 1x ModuleLoadException: At offset N: At offset N: Don't know how to parse opcode "N".
        var skips = new HashSet<uint>
        {
            1, 183, 184, 185, 186, 188, 189, 190, 191, 193, 194, 195, 196, 197, 198, 199, 200, 202, 203,
            204, 205, 206, 207, 208, 209, 211, 212, 213, 214, 215, 216, 218, 219, 220, 221, 222, 223, 225,
            226, 227, 228, 229, 230, 231, 232, 234, 235, 236, 237, 238, 239, 240, 241, 243, 244, 245, 246,
            247, 248, 250, 251, 252, 253, 254, 255, 257, 258, 259, 260, 261, 262, 264, 265, 266, 267, 269,
            270, 271, 272, 274, 275, 276, 277, 278, 279, 281, 282, 283, 284, 286, 287, 289, 290, 291, 292,
            293, 294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 306, 307, 308, 309, 310, 311, 312,
            313, 314, 315, 316, 317, 324, 340, 518
        };
        SpecTestRunner.Run(DataPath("select"), "select.json", skips.Contains);
    }

    /// <summary>Runs the skip stack guard page tests.</summary>
    [TestMethod]
    [Ignore("Causes CLR malfunction.")]
    public void SpecTest_skip_stack_guard_page() => SpecTestRunner.Run(DataPath("skip-stack-guard-page"), "skip-stack-guard-page.json");

    /// <summary>Runs the stack tests.</summary>
    [TestMethod]
    public void SpecTest_stack() => SpecTestRunner.Run(DataPath("stack"), "stack.json");

    /// <summary>Runs the start tests.</summary>
    [TestMethod]
    public void SpecTest_start()
    {
        // 2x AssertFailedException: start function doesn't have a test procedure set up.
        // 1x ImportException: Missing import for spectest::print.
        // 1x AssertFailedException: expected ModuleLoadException not thrown
        var skips = new HashSet<uint>
        {
            7, 14, 92, 98
        };
        SpecTestRunner.Run(DataPath("start"), "start.json", skips.Contains);
    }

    /// <summary>Runs the store tests.</summary>
    [TestMethod]
    public void SpecTest_store() => SpecTestRunner.Run(DataPath("store"), "store.json");

    /// <summary>Runs the switch tests.</summary>
    [TestMethod]
    public void SpecTest_switch()
    {
        // 9x AssertFailedException: Common Language Runtime detected an invalid program.
        var skips = new HashSet<uint>
        {
            138, 139, 140, 141, 142, 143, 144, 145, 146
        };
        SpecTestRunner.Run(DataPath("switch"), "switch.json", skips.Contains);
    }

    /// <summary>Runs the table tests.</summary>
    [TestMethod]
    public void SpecTest_table()
    {
        // 2x ModuleLoadException: At offset N: Stream ended unexpectedly.
        // 2x AssertFailedException: expected ModuleLoadException not thrown
        var skips = new HashSet<uint>
        {
            11, 12, 19, 23
        };
        SpecTestRunner.Run(DataPath("table"), "table.json", skips.Contains);
    }

    /// <summary>Runs the token tests.</summary>
    [TestMethod]
    public void SpecTest_token()
    {
        // 1x ImportException: Missing import for spectest::print.
        // 3x ModuleLoadException: At offset N: At offset N: Opcode "NoOperation" is not permitted in intializer expressions.
        // 3x ModuleLoadException: At offset N: At offset N: Opcode "Block" is not permitted in intializer expressions.
        // 3x ModuleLoadException: At offset N: At offset N: Opcode "Loop" is not permitted in intializer expressions.
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "6" is not permitted in intializer expressions.
        // 2x ModuleLoadException: At offset N: At offset N: Opcode "7" is not permitted in intializer expressions.
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "Branch" is not permitted in intializer expressions.
        // 2x ModuleLoadException: At offset N: At offset N: Opcode "BranchIf" is not permitted in intializer expressions.
        var skips = new HashSet<uint>
        {
            47, 74, 122, 132, 142, 152, 162, 172, 182, 192, 202, 212, 222, 232, 242, 252
        };
        SpecTestRunner.Run(DataPath("token"), "token.json", skips.Contains);
    }

    /// <summary>Runs the traps tests.</summary>
    [TestMethod]
    public void SpecTest_traps() => SpecTestRunner.Run(DataPath("traps"), "traps.json");

    /// <summary>Runs the type tests.</summary>
    [TestMethod]
    public void SpecTest_type()
    {
        // 1x ModuleLoadException: At offset N: WebAssemblyValueType N not recognized. (Parameter 'valueType')
        var skips = new HashSet<uint>
        {
            3
        };
        SpecTestRunner.Run(DataPath("type"), "type.json", skips.Contains);
    }

    /// <summary>Runs the unreachable tests.</summary>
    [TestMethod]
    public void SpecTest_unreachable()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Opcode "Block" is not permitted in intializer expressions.
        // 58x AssertFailedException: got AssertFailedException, expected UnreachableException
        // 5x AssertFailedException: no method source (cascaded module-load failure)
        var skips = new HashSet<uint>
        {
            3, 221, 222, 223, 224, 226, 227, 228, 229, 231, 232, 233, 234, 235, 237, 238, 239, 240, 242,
            244, 245, 246, 248, 249, 250, 251, 252, 254, 256, 257, 258, 259, 260, 261, 262, 264, 265, 266,
            267, 268, 270, 271, 272, 274, 275, 276, 277, 279, 280, 281, 283, 284, 286, 287, 288, 289, 291,
            293, 294, 296, 298, 299, 301, 303
        };
        SpecTestRunner.Run(DataPath("unreachable"), "unreachable.json", skips.Contains);
    }

    /// <summary>Runs the unreached invalid tests.</summary>
    [TestMethod]
    public void SpecTest_unreached_invalid()
    {
        // 2x AssertFailedException: expected exception not thrown (validation issues with unreachable code)
        var skips = new HashSet<uint>
        {
            490, 676
        };
        SpecTestRunner.Run(DataPath("unreached-invalid"), "unreached-invalid.json", skips.Contains);
    }

    /// <summary>Runs the unreached valid tests.</summary>
    [TestMethod]
    public void SpecTest_unreached_valid()
    {
        // 1x ModuleLoadException: At offset N: At offset N: Don't know how to parse opcode "N".
        // 5x AssertFailedException: got AssertFailedException, expected UnreachableException
        // 1x LabelTypeMismatchException: BranchTable requires all labels to have type Float64, but found Float32.
        var skips = new HashSet<uint>
        {
            1, 42, 43, 44, 45, 49, 63
        };
        SpecTestRunner.Run(DataPath("unreached-valid"), "unreached-valid.json", skips.Contains);
    }

    /// <summary>Runs the unwind tests.</summary>
    [TestMethod]
    [Ignore("The JIT compiler encountered invalid IL code or an internal limitation.")]
    public void SpecTest_unwind() => SpecTestRunner.Run(DataPath("unwind"), "unwind.json");

    /// <summary>Runs the utf8 custom section id tests.</summary>
    [TestMethod]
    public void SpecTest_utf8_custom_section_id() => SpecTestRunner.Run(DataPath("utf8-custom-section-id"), "utf8-custom-section-id.json");

    /// <summary>Runs the utf8 import field tests.</summary>
    [TestMethod]
    public void SpecTest_utf8_import_field() => SpecTestRunner.Run(DataPath("utf8-import-field"), "utf8-import-field.json");

    /// <summary>Runs the utf8 import module tests.</summary>
    [TestMethod]
    public void SpecTest_utf8_import_module() => SpecTestRunner.Run(DataPath("utf8-import-module"), "utf8-import-module.json");

    /// <summary>Runs the utf8 invalid encoding tests.</summary>
    [TestMethod]
    public void SpecTest_utf8_invalid_encoding() => SpecTestRunner.Run(DataPath("utf8-invalid-encoding"), "utf8-invalid-encoding.json");


    // ==== WASM 2.0 — Bulk memory operations ====

    /// <summary>Runs the bulk tests.</summary>
    [TestMethod]
    public void SpecTest_bulk() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "bulk"), "bulk.json");

    /// <summary>Runs the memory_copy tests.</summary>
    [TestMethod]
    public void SpecTest_memory_copy() => SpecTestRunner.Run(DataPath("memory_copy"), "memory_copy.json");

    /// <summary>Runs the memory_fill tests.</summary>
    [TestMethod]
    public void SpecTest_memory_fill() => SpecTestRunner.Run(DataPath("memory_fill"), "memory_fill.json");

    /// <summary>Runs the memory_init tests.</summary>
    [TestMethod]
    public void SpecTest_memory_init()
    {
        // 3x AssertFailedException: unknown data segment doesn't have a test procedure set up.
        var skips = new HashSet<uint> { 190, 196, 233 };
        SpecTestRunner.Run(DataPath("memory_init"), "memory_init.json", skips.Contains);
    }

    /// <summary>Runs the table_copy tests.</summary>
    [TestMethod]
    public void SpecTest_table_copy() => SpecTestRunner.Run(DataPath("table_copy"), "table_copy.json");

    /// <summary>Runs the table_fill tests.</summary>
    [TestMethod]
    public void SpecTest_table_fill() => SpecTestRunner.Run(DataPath("table_fill"), "table_fill.json");

    /// <summary>Runs the table_init tests.</summary>
    [TestMethod]
    public void SpecTest_table_init() => SpecTestRunner.Run(DataPath("table_init"), "table_init.json");


    // ==== WASM 2.0 — Reference types & multi-table ====

    /// <summary>Runs the ref_func tests.</summary>
    [TestMethod]
    public void SpecTest_ref_func() => SpecTestRunner.Run(DataPath("ref_func"), "ref_func.json");

    /// <summary>Runs the ref_is_null tests.</summary>
    [TestMethod]
    public void SpecTest_ref_is_null() => SpecTestRunner.Run(DataPath("ref_is_null"), "ref_is_null.json");

    /// <summary>Runs the ref_null tests.</summary>
    [TestMethod]
    public void SpecTest_ref_null() => SpecTestRunner.Run(DataPath("ref_null"), "ref_null.json");

    /// <summary>Runs the table sub tests.</summary>
    [TestMethod]
    public void SpecTest_table_sub() => SpecTestRunner.Run(DataPath("table-sub"), "table-sub.json");

    /// <summary>Runs the table_get tests.</summary>
    [TestMethod]
    public void SpecTest_table_get() => SpecTestRunner.Run(DataPath("table_get"), "table_get.json");

    /// <summary>Runs the table_grow tests.</summary>
    [TestMethod]
    public void SpecTest_table_grow() => SpecTestRunner.Run(DataPath("table_grow"), "table_grow.json");

    /// <summary>Runs the table_set tests.</summary>
    [TestMethod]
    public void SpecTest_table_set() => SpecTestRunner.Run(DataPath("table_set"), "table_set.json");

    /// <summary>Runs the table_size tests.</summary>
    [TestMethod]
    public void SpecTest_table_size() => SpecTestRunner.Run(DataPath("table_size"), "table_size.json");


    // ==== WASM 2.0 — SIMD (fixed-width 128-bit) ====

    /// <summary>Runs the simd_address tests.</summary>
    [TestMethod]
    public void SpecTest_simd_address() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_address"), "simd_address.json");

    /// <summary>Runs the simd_align tests.</summary>
    [TestMethod]
    public void SpecTest_simd_align() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_align"), "simd_align.json");

    /// <summary>Runs the simd_bit_shift tests.</summary>
    [TestMethod]
    public void SpecTest_simd_bit_shift() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_bit_shift"), "simd_bit_shift.json");

    /// <summary>Runs the simd_bitwise tests.</summary>
    [TestMethod]
    public void SpecTest_simd_bitwise() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_bitwise"), "simd_bitwise.json");

    /// <summary>Runs the simd_boolean tests.</summary>
    [TestMethod]
    public void SpecTest_simd_boolean() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_boolean"), "simd_boolean.json");

    /// <summary>Runs the simd_const tests.</summary>
    [TestMethod]
    public void SpecTest_simd_const() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_const"), "simd_const.json");

    /// <summary>Runs the simd_conversions tests.</summary>
    [TestMethod]
    public void SpecTest_simd_conversions() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_conversions"), "simd_conversions.json");

    /// <summary>Runs the simd_f32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4"), "simd_f32x4.json");

    /// <summary>Runs the simd_f32x4_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4_arith"), "simd_f32x4_arith.json");

    /// <summary>Runs the simd_f32x4_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4_cmp"), "simd_f32x4_cmp.json");

    /// <summary>Runs the simd_f32x4_pmin_pmax tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_pmin_pmax() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4_pmin_pmax"), "simd_f32x4_pmin_pmax.json");

    /// <summary>Runs the simd_f32x4_rounding tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_rounding() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4_rounding"), "simd_f32x4_rounding.json");

    /// <summary>Runs the simd_f64x2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2"), "simd_f64x2.json");

    /// <summary>Runs the simd_f64x2_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2_arith"), "simd_f64x2_arith.json");

    /// <summary>Runs the simd_f64x2_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2_cmp"), "simd_f64x2_cmp.json");

    /// <summary>Runs the simd_f64x2_pmin_pmax tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_pmin_pmax() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2_pmin_pmax"), "simd_f64x2_pmin_pmax.json");

    /// <summary>Runs the simd_f64x2_rounding tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_rounding() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2_rounding"), "simd_f64x2_rounding.json");

    /// <summary>Runs the simd_i16x8_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_arith"), "simd_i16x8_arith.json");

    /// <summary>Runs the simd_i16x8_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_arith2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_arith2"), "simd_i16x8_arith2.json");

    /// <summary>Runs the simd_i16x8_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_cmp"), "simd_i16x8_cmp.json");

    /// <summary>Runs the simd_i16x8_extadd_pairwise_i8x16 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_extadd_pairwise_i8x16() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_extadd_pairwise_i8x16"), "simd_i16x8_extadd_pairwise_i8x16.json");

    /// <summary>Runs the simd_i16x8_extmul_i8x16 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_extmul_i8x16() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_extmul_i8x16"), "simd_i16x8_extmul_i8x16.json");

    /// <summary>Runs the simd_i16x8_q15mulr_sat_s tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_q15mulr_sat_s() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_q15mulr_sat_s"), "simd_i16x8_q15mulr_sat_s.json");

    /// <summary>Runs the simd_i16x8_sat_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_sat_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_sat_arith"), "simd_i16x8_sat_arith.json");

    /// <summary>Runs the simd_i32x4_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_arith"), "simd_i32x4_arith.json");

    /// <summary>Runs the simd_i32x4_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_arith2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_arith2"), "simd_i32x4_arith2.json");

    /// <summary>Runs the simd_i32x4_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_cmp"), "simd_i32x4_cmp.json");

    /// <summary>Runs the simd_i32x4_dot_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_dot_i16x8() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_dot_i16x8"), "simd_i32x4_dot_i16x8.json");

    /// <summary>Runs the simd_i32x4_extadd_pairwise_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_extadd_pairwise_i16x8() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_extadd_pairwise_i16x8"), "simd_i32x4_extadd_pairwise_i16x8.json");

    /// <summary>Runs the simd_i32x4_extmul_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_extmul_i16x8() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_extmul_i16x8"), "simd_i32x4_extmul_i16x8.json");

    /// <summary>Runs the simd_i32x4_trunc_sat_f32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_trunc_sat_f32x4() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_trunc_sat_f32x4"), "simd_i32x4_trunc_sat_f32x4.json");

    /// <summary>Runs the simd_i32x4_trunc_sat_f64x2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_trunc_sat_f64x2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_trunc_sat_f64x2"), "simd_i32x4_trunc_sat_f64x2.json");

    /// <summary>Runs the simd_i64x2_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i64x2_arith"), "simd_i64x2_arith.json");

    /// <summary>Runs the simd_i64x2_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_arith2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i64x2_arith2"), "simd_i64x2_arith2.json");

    /// <summary>Runs the simd_i64x2_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i64x2_cmp"), "simd_i64x2_cmp.json");

    /// <summary>Runs the simd_i64x2_extmul_i32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_extmul_i32x4() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i64x2_extmul_i32x4"), "simd_i64x2_extmul_i32x4.json");

    /// <summary>Runs the simd_i8x16_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i8x16_arith"), "simd_i8x16_arith.json");

    /// <summary>Runs the simd_i8x16_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_arith2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i8x16_arith2"), "simd_i8x16_arith2.json");

    /// <summary>Runs the simd_i8x16_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i8x16_cmp"), "simd_i8x16_cmp.json");

    /// <summary>Runs the simd_i8x16_sat_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_sat_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i8x16_sat_arith"), "simd_i8x16_sat_arith.json");

    /// <summary>Runs the simd_int_to_int_extend tests.</summary>
    [TestMethod]
    public void SpecTest_simd_int_to_int_extend() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_int_to_int_extend"), "simd_int_to_int_extend.json");

    /// <summary>Runs the simd_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_lane"), "simd_lane.json");

    /// <summary>Runs the simd_linking tests.</summary>
    [TestMethod]
    [Ignore("simd_linking data only in simd/ subdirectory")]
    public void SpecTest_simd_linking() => SpecTestRunner.Run(DataPath("simd", "simd_linking"), "simd_linking.json");

    /// <summary>Runs the simd_load tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load"), "simd_load.json");

    /// <summary>Runs the simd_load8_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load8_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load8_lane"), "simd_load8_lane.json");

    /// <summary>Runs the simd_load16_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load16_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load16_lane"), "simd_load16_lane.json");

    /// <summary>Runs the simd_load32_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load32_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load32_lane"), "simd_load32_lane.json");

    /// <summary>Runs the simd_load64_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load64_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load64_lane"), "simd_load64_lane.json");

    /// <summary>Runs the simd_load_extend tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_extend() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load_extend"), "simd_load_extend.json");

    /// <summary>Runs the simd_load_splat tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_splat() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load_splat"), "simd_load_splat.json");

    /// <summary>Runs the simd_load_zero tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_zero() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load_zero"), "simd_load_zero.json");

    /// <summary>Runs the simd_select tests.</summary>
    [TestMethod]
    public void SpecTest_simd_select() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_select"), "simd_select.json");

    /// <summary>Runs the simd_splat tests.</summary>
    [TestMethod]
    public void SpecTest_simd_splat() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_splat"), "simd_splat.json");

    /// <summary>Runs the simd_store tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store"), "simd_store.json");

    /// <summary>Runs the simd_store8_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store8_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store8_lane"), "simd_store8_lane.json");

    /// <summary>Runs the simd_store16_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store16_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store16_lane"), "simd_store16_lane.json");

    /// <summary>Runs the simd_store32_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store32_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store32_lane"), "simd_store32_lane.json");

    /// <summary>Runs the simd_store64_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store64_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store64_lane"), "simd_store64_lane.json");
}
