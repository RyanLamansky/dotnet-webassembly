using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Call"/> instruction.
    /// </summary>
    [TestClass]
    public class CallTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Call"/> instruction.
        /// </summary>
        [TestMethod]
        public void Call_Compiled()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = new[]
                {
                    WebAssemblyValueType.Int32,
                },
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                }
            });
            module.Functions.Add(new Function
            {
            });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = nameof(CompilerTestBase<int>.Test)
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new LocalGet(0),
                    new Call(1),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new LocalGet(0),
                    new Int32Constant(1),
                    new Int32Add(),
                    new End(),
                },
            });

            var compiled = module.ToInstance<CompilerTestBase<int>>();

            var exports = compiled.Exports;
            Assert.AreEqual(1, exports.Test(0));
            Assert.AreEqual(4, exports.Test(3));
        }

        /// <summary>
        /// Used by <see cref="Call_Compiled_MixedParameterTypes"/>.
        /// </summary>
        public abstract class MixedParameters
        {
            /// <summary>
            /// Returns a value.
            /// </summary>
            /// <param name="parameter1">Input to the test function.</param>
            /// <param name="parameter2">Input to the test function.</param>
            /// <returns>A value to ensure proper control flow and execution.</returns>
            public abstract int Test(int parameter1, double parameter2);
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Call"/> instruction with mixed parameter types.
        /// </summary>
        [TestMethod]
        public void Call_Compiled_MixedParameterTypes()
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = new[]
                {
                    WebAssemblyValueType.Int32,
                    WebAssemblyValueType.Float64, //Not actually used, just here to verify that the call works correctly.
				},
                Returns = new[]
                {
                    WebAssemblyValueType.Int32,
                }
            });
            module.Functions.Add(new Function
            {
            });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = nameof(MixedParameters.Test)
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new LocalGet(0),
                    new LocalGet(1),
                    new Call(1),
                    new End(),
                },
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new LocalGet(0),
                    new Int32Constant(1),
                    new Int32Add(),
                    new End(),
                },
            });

            var compiled = module.ToInstance<MixedParameters>();

            var exports = compiled.Exports;
            Assert.AreEqual(1, exports.Test(0, 0));
            Assert.AreEqual(4, exports.Test(3, 3));
        }

        /// <summary>
        /// Tests the <see cref="Call.Equals(Instruction)"/> and <see cref="Call.GetHashCode()"/> methods.
        /// </summary>
        [TestMethod]
        public void Call_Equals()
        {
            TestUtility.CreateInstances<Call>(out var a, out var b);
            a.Index = 1;
            TestUtility.AreNotEqual(a, b);
            b.Index = 1;
            TestUtility.AreEqual(a, b);
        }
    }
}