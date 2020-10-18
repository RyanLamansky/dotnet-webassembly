using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WebAssembly
{
    /// <summary>
    /// Tests that <see cref="OpCode"/> names and values are set up correctly.
    /// </summary>
    [TestClass]
    public class OpCodeTests
    {
        private static readonly Dictionary<OpCode, OpCodeCharacteristicsAttribute?> opCodeCharacteristicsByOpCode =
            typeof(OpCode)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => (field.Attributes & FieldAttributes.Literal) != 0 && (field.Attributes & FieldAttributes.HasDefault) != 0)
            .ToDictionary(field => (OpCode)field.GetRawConstantValue()!, field => field.GetCustomAttribute<OpCodeCharacteristicsAttribute>());

        /// <summary>
        /// Ensures that the name of an opcode is appropriate for the name assigned to its associated <see cref="OpCodeCharacteristicsAttribute"/>.
        /// </summary>
        [TestMethod]
        public void OpCode_NameMatchesCharacteristics()
        {
            var splitter = new[] { '.', '_' };
            var replacements = new Dictionary<string, string>
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
            };

            foreach (var kv in opCodeCharacteristicsByOpCode)
            {
                var opCode = kv.Key;
                var characteristics = kv.Value;
                var expectedName = new StringBuilder();

                Assert.IsNotNull(characteristics);

                var parts = characteristics!.Name.Split(splitter);
                foreach (var part in parts)
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

                    expectedName.Append(char.ToUpper(part[0])).Append(part.Substring(1));
                }

                Assert.AreEqual(expectedName.ToString(), opCode.ToString());
            }
        }
    }
}
