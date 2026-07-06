using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WebAssembly;

/// <summary>
/// Tests that the names of the opcode enumerations (<see cref="OpCode"/>, <see cref="MiscellaneousOpCode"/>,
/// <see cref="SimdOpCode"/>, and <see cref="GCOpCode"/>) are derived from their native (standardized) names
/// according to a single house naming convention.
/// </summary>
[TestClass]
public class OpCodeTests
{
    // The house convention: each native name (e.g. "i32.trunc_sat_f64_u") is split on '.' and '_', each part is
    // either expanded via this table or title-cased, and the parts are concatenated (e.g. "Int32TruncateSaturateFloat64Unsigned").
    // The core OpCode enum is the reference standard; these replacements are its expansions plus the additional
    // tokens introduced by the miscellaneous, SIMD, and GC enums (kept consistent with the core spellings).
    private static readonly Dictionary<string, string> replacements = new()
    {
        { "i32", "Int32" },
        { "i64", "Int64" },
        { "f32", "Float32" },
        { "f64", "Float64" },
        { "nop", "NoOperation" },
        { "br", "Branch" },
        { "s", "Signed" },
        { "u", "Unsigned" },
        { "ne", "NotEqual" },
        { "lt", "LessThan" },
        { "gt", "GreaterThan" },
        { "le", "LessThanOrEqual" },
        { "ge", "GreaterThanOrEqual" },
        { "clz", "CountLeadingZeroes" },
        { "ctz", "CountTrailingZeroes" },
        { "popcnt", "CountOneBits" },
        { "sub", "Subtract" },
        { "mul", "Multiply" },
        { "div", "Divide" },
        { "rem", "Remainder" },
        { "xor", "ExclusiveOr" },
        { "shl", "ShiftLeft" },
        { "shr", "ShiftRight" },
        { "rotl", "RotateLeft" },
        { "rotr", "RotateRight" },
        { "abs", "Absolute" },
        { "neg", "Negate" },
        { "ceil", "Ceiling" },
        { "trunc", "Truncate" },
        { "sqrt", "SquareRoot" },
        { "min", "Minimum" },
        { "max", "Maximum" },
        { "copysign", "CopySign" },
        { "const", "Constant" },
        { "misc", "MiscellaneousOperationPrefix" },
        { "simd", "SimdOperationPrefix" },
        { "gc", "GCPrefix" },
        // Additional tokens introduced by the miscellaneous and SIMD enums.
        { "sat", "Saturate" },
        { "i8x16", "Int8x16" },
        { "i16x8", "Int16x8" },
        { "i32x4", "Int32x4" },
        { "i64x2", "Int64x2" },
        { "f32x4", "Float32x4" },
        { "f64x2", "Float64x2" },
        { "i7x16", "Int7x16" },
        { "v128", "V128" },
        { "madd", "MultiplyAdd" },
        { "nmadd", "NegativeMultiplyAdd" },
        { "q15mulr", "Q15Mulr" },
    };

    private static string ExpectedName(string nativeName)
    {
        var expectedName = new StringBuilder();

        foreach (var part in nativeName.Split('.', '_'))
        {
            if (replacements.TryGetValue(part, out var toAppend))
            {
                expectedName.Append(toAppend);
                continue;
            }

            if (part.StartsWith("eq"))
            {
                expectedName.Append("Equal");
                if (part.Length >= 3 && part[2] == 'z')
                    expectedName.Append("Zero");

                continue;
            }

            expectedName.Append(char.ToUpper(part[0])).Append(part[1..]);
        }

        return expectedName.ToString();
    }

    /// <summary>
    /// Verifies that every member of <typeparamref name="TEnum"/> carries an <see cref="OpCodeCharacteristicsAttribute"/>
    /// whose native name expands, under the house convention, to the member's own name.
    /// </summary>
    /// <typeparam name="TEnum">The opcode enumeration to check.</typeparam>
    /// <param name="exemptions">
    /// Released members whose names deviate from the convention, mapped to their convention-correct spelling.
    /// The exemption keeps the test green today while locking the convention for all future additions; each entry
    /// is verified to still deviate (so stale exemptions are caught) and to still expand to the documented name.
    /// </param>
    private static void AssertNamesMatchCharacteristics<TEnum>(IReadOnlyDictionary<string, string>? exemptions = null)
        where TEnum : struct, Enum
    {
        exemptions ??= new Dictionary<string, string>();
        var observedExemptions = new HashSet<string>();

        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var member = field.Name;
            var characteristics = field.GetCustomAttribute<OpCodeCharacteristicsAttribute>();
            Assert.IsNotNull(characteristics, $"{typeof(TEnum).Name}.{member} is missing {nameof(OpCodeCharacteristicsAttribute)}.");

            var derivedName = ExpectedName(characteristics!.Name);

            if (exemptions.TryGetValue(member, out var conventionCorrect))
            {
                Assert.AreNotEqual(derivedName, member,
                    $"{typeof(TEnum).Name}.{member} now matches the convention; remove it from the exemption list.");
                Assert.AreEqual(conventionCorrect, derivedName,
                    $"The documented convention-correct name for {typeof(TEnum).Name}.{member} is out of date.");
                observedExemptions.Add(member);
                continue;
            }

            Assert.AreEqual(derivedName, member,
                $"{typeof(TEnum).Name} member for native name '{characteristics.Name}' should be '{derivedName}'.");
        }

        var stale = exemptions.Keys.Except(observedExemptions).ToList();
        Assert.AreEqual(0, stale.Count, $"Exemptions reference unknown {typeof(TEnum).Name} members: {string.Join(", ", stale)}");
    }

    /// <summary>
    /// Ensures core <see cref="OpCode"/> names match their <see cref="OpCodeCharacteristicsAttribute"/> under the house convention.
    /// </summary>
    [TestMethod]
    public void OpCode_NameMatchesCharacteristics() => AssertNamesMatchCharacteristics<OpCode>();

    /// <summary>
    /// Ensures <see cref="MiscellaneousOpCode"/> names match their <see cref="OpCodeCharacteristicsAttribute"/> under the house convention.
    /// </summary>
    [TestMethod]
    public void MiscellaneousOpCode_NameMatchesCharacteristics() => AssertNamesMatchCharacteristics<MiscellaneousOpCode>();

    /// <summary>
    /// Ensures <see cref="GCOpCode"/> names match their <see cref="OpCodeCharacteristicsAttribute"/> under the house convention.
    /// </summary>
    [TestMethod]
    public void GCOpCode_NameMatchesCharacteristics() => AssertNamesMatchCharacteristics<GCOpCode>();

    /// <summary>
    /// Ensures <see cref="SimdOpCode"/> names match their <see cref="OpCodeCharacteristicsAttribute"/> under the house convention.
    /// </summary>
    /// <remarks>
    /// The WASM 2.0 SIMD members were shipped (NuGet 2.0.0) before this convention was test-verified, so they use
    /// abbreviated spellings (Abs, Neg, Min, Max, Sub, Mul, Div, Sqrt, Ceil, Trunc, TruncSat, Popcnt, Xor, Const, …)
    /// that the core enum expands. They are grandfathered via <see cref="simdReleasedExemptions"/> pending a possible
    /// breaking-change release; the mapped value is the convention-correct name a future release would adopt. The
    /// relaxed-SIMD members (WASM 3.0, unreleased) were corrected to follow the convention and carry no exemption.
    /// </remarks>
    [TestMethod]
    public void SimdOpCode_NameMatchesCharacteristics() => AssertNamesMatchCharacteristics<SimdOpCode>(simdReleasedExemptions);

    // Released WASM 2.0 SIMD member -> convention-correct name. Do not change the members (breaking); do not add
    // relaxed-SIMD or other WASM 3.0 members here (those must conform to the convention outright).
    private static readonly Dictionary<string, string> simdReleasedExemptions = new()
    {
        { "V128Load8X8Signed", "V128Load8x8Signed" }, // v128.load8x8_s
        { "V128Load8X8Unsigned", "V128Load8x8Unsigned" }, // v128.load8x8_u
        { "V128Load16X4Signed", "V128Load16x4Signed" }, // v128.load16x4_s
        { "V128Load16X4Unsigned", "V128Load16x4Unsigned" }, // v128.load16x4_u
        { "V128Load32X2Signed", "V128Load32x2Signed" }, // v128.load32x2_s
        { "V128Load32X2Unsigned", "V128Load32x2Unsigned" }, // v128.load32x2_u
        { "V128Const", "V128Constant" }, // v128.const
        { "V128AndNot", "V128Andnot" }, // v128.andnot
        { "V128Xor", "V128ExclusiveOr" }, // v128.xor
        { "Int8x16Abs", "Int8x16Absolute" }, // i8x16.abs
        { "Int8x16Neg", "Int8x16Negate" }, // i8x16.neg
        { "Int8x16Popcnt", "Int8x16CountOneBits" }, // i8x16.popcnt
        { "Int8x16Sub", "Int8x16Subtract" }, // i8x16.sub
        { "Int8x16SubSaturateSigned", "Int8x16SubtractSaturateSigned" }, // i8x16.sub_sat_s
        { "Int8x16SubSaturateUnsigned", "Int8x16SubtractSaturateUnsigned" }, // i8x16.sub_sat_u
        { "Int8x16MinSigned", "Int8x16MinimumSigned" }, // i8x16.min_s
        { "Int8x16MinUnsigned", "Int8x16MinimumUnsigned" }, // i8x16.min_u
        { "Int8x16MaxSigned", "Int8x16MaximumSigned" }, // i8x16.max_s
        { "Int8x16MaxUnsigned", "Int8x16MaximumUnsigned" }, // i8x16.max_u
        { "Int16x8Abs", "Int16x8Absolute" }, // i16x8.abs
        { "Int16x8Neg", "Int16x8Negate" }, // i16x8.neg
        { "Int16x8Q15MulrSatSigned", "Int16x8Q15MulrSaturateSigned" }, // i16x8.q15mulr_sat_s
        { "Int16x8Sub", "Int16x8Subtract" }, // i16x8.sub
        { "Int16x8SubSaturateSigned", "Int16x8SubtractSaturateSigned" }, // i16x8.sub_sat_s
        { "Int16x8SubSaturateUnsigned", "Int16x8SubtractSaturateUnsigned" }, // i16x8.sub_sat_u
        { "Int16x8Mul", "Int16x8Multiply" }, // i16x8.mul
        { "Int16x8MinSigned", "Int16x8MinimumSigned" }, // i16x8.min_s
        { "Int16x8MinUnsigned", "Int16x8MinimumUnsigned" }, // i16x8.min_u
        { "Int16x8MaxSigned", "Int16x8MaximumSigned" }, // i16x8.max_s
        { "Int16x8MaxUnsigned", "Int16x8MaximumUnsigned" }, // i16x8.max_u
        { "Int32x4Abs", "Int32x4Absolute" }, // i32x4.abs
        { "Int32x4Neg", "Int32x4Negate" }, // i32x4.neg
        { "Int32x4Sub", "Int32x4Subtract" }, // i32x4.sub
        { "Int32x4Mul", "Int32x4Multiply" }, // i32x4.mul
        { "Int32x4MinSigned", "Int32x4MinimumSigned" }, // i32x4.min_s
        { "Int32x4MinUnsigned", "Int32x4MinimumUnsigned" }, // i32x4.min_u
        { "Int32x4MaxSigned", "Int32x4MaximumSigned" }, // i32x4.max_s
        { "Int32x4MaxUnsigned", "Int32x4MaximumUnsigned" }, // i32x4.max_u
        { "Int64x2Abs", "Int64x2Absolute" }, // i64x2.abs
        { "Int64x2Neg", "Int64x2Negate" }, // i64x2.neg
        { "Int64x2Sub", "Int64x2Subtract" }, // i64x2.sub
        { "Int64x2Mul", "Int64x2Multiply" }, // i64x2.mul
        { "Float32x4Ceil", "Float32x4Ceiling" }, // f32x4.ceil
        { "Float32x4Trunc", "Float32x4Truncate" }, // f32x4.trunc
        { "Float32x4Abs", "Float32x4Absolute" }, // f32x4.abs
        { "Float32x4Neg", "Float32x4Negate" }, // f32x4.neg
        { "Float32x4Sqrt", "Float32x4SquareRoot" }, // f32x4.sqrt
        { "Float32x4Sub", "Float32x4Subtract" }, // f32x4.sub
        { "Float32x4Mul", "Float32x4Multiply" }, // f32x4.mul
        { "Float32x4Div", "Float32x4Divide" }, // f32x4.div
        { "Float32x4Min", "Float32x4Minimum" }, // f32x4.min
        { "Float32x4Max", "Float32x4Maximum" }, // f32x4.max
        { "Float64x2Ceil", "Float64x2Ceiling" }, // f64x2.ceil
        { "Float64x2Trunc", "Float64x2Truncate" }, // f64x2.trunc
        { "Float64x2Abs", "Float64x2Absolute" }, // f64x2.abs
        { "Float64x2Neg", "Float64x2Negate" }, // f64x2.neg
        { "Float64x2Sqrt", "Float64x2SquareRoot" }, // f64x2.sqrt
        { "Float64x2Sub", "Float64x2Subtract" }, // f64x2.sub
        { "Float64x2Mul", "Float64x2Multiply" }, // f64x2.mul
        { "Float64x2Div", "Float64x2Divide" }, // f64x2.div
        { "Float64x2Min", "Float64x2Minimum" }, // f64x2.min
        { "Float64x2Max", "Float64x2Maximum" }, // f64x2.max
        { "Int32x4TruncSatFloat32x4Signed", "Int32x4TruncateSaturateFloat32x4Signed" }, // i32x4.trunc_sat_f32x4_s
        { "Int32x4TruncSatFloat32x4Unsigned", "Int32x4TruncateSaturateFloat32x4Unsigned" }, // i32x4.trunc_sat_f32x4_u
        { "Int32x4TruncSatFloat64x2SignedZero", "Int32x4TruncateSaturateFloat64x2SignedZero" }, // i32x4.trunc_sat_f64x2_s_zero
        { "Int32x4TruncSatFloat64x2UnsignedZero", "Int32x4TruncateSaturateFloat64x2UnsignedZero" }, // i32x4.trunc_sat_f64x2_u_zero
    };
}
