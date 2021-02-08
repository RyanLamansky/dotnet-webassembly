using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Select"/> instruction.
    /// </summary>
    [TestClass]
    public class SelectTests
    {
        /// <summary>
        /// Tests the <see cref="Select"/> instruction.
        /// </summary>
        /// <typeparam name="T">The value to be returned.</typeparam>
        public abstract class SelectTester<T>
            where T : struct
        {
            /// <summary>
            /// Tests the <see cref="Select"/> instruction.
            /// </summary>
            public abstract T Test(T a, T b, int c);
        }

        static Instance<SelectTester<T>> CreateTester<T>(WebAssemblyValueType type)
            where T : struct
        {
            var module = new Module();
            module.Types.Add(new WebAssemblyType
            {
                Parameters = new WebAssemblyValueType[]
                {
                    type,
                    type,
                    WebAssemblyValueType.Int32,
                },
                Returns = new[]
                {
                    type,
                },
            });
            module.Functions.Add(new Function
            {
            });
            module.Exports.Add(new Export
            {
                Name = nameof(SelectTester<T>.Test),
            });
            module.Codes.Add(new FunctionBody
            {
                Code = new Instruction[]
                {
                    new LocalGet(0),
                    new LocalGet(1),
                    new LocalGet(2),
                    new Select(),
                    new End(),
                },
            });

            return module.ToInstance<SelectTester<T>>();
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.Int32"/> input.
        /// </summary>
        [TestMethod]
        public void Select_Compiled_Int32()
        {
            using var compiled = CreateTester<int>(WebAssemblyValueType.Int32);
            var exports = compiled.Exports;
            Assert.AreEqual(1, exports.Test(1, 2, 3));
            Assert.AreEqual(2, exports.Test(1, 2, 0));
            Assert.AreEqual(4, exports.Test(4, 5, 1));
            Assert.AreEqual(5, exports.Test(4, 5, 0));
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.Int64"/> input.
        /// </summary>
        [TestMethod]
        public void Select_Compiled_Int64()
        {
            using var compiled = CreateTester<long>(WebAssemblyValueType.Int64);
            var exports = compiled.Exports;
            Assert.AreEqual(1, exports.Test(1, 2, 3));
            Assert.AreEqual(2, exports.Test(1, 2, 0));
            Assert.AreEqual(4, exports.Test(4, 5, 1));
            Assert.AreEqual(5, exports.Test(4, 5, 0));
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.Float32"/> input.
        /// </summary>
        [TestMethod]
        public void Select_Compiled_Float32()
        {
            using var compiled = CreateTester<float>(WebAssemblyValueType.Float32);
            var exports = compiled.Exports;
            Assert.AreEqual(1, exports.Test(1, 2, 3));
            Assert.AreEqual(2, exports.Test(1, 2, 0));
            Assert.AreEqual(4, exports.Test(4, 5, 1));
            Assert.AreEqual(5, exports.Test(4, 5, 0));
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.Float64"/> input.
        /// </summary>
        [TestMethod]
        public void Select_Compiled_Float64()
        {
            using var compiled = CreateTester<double>(WebAssemblyValueType.Float64);
            var exports = compiled.Exports;
            Assert.AreEqual(1, exports.Test(1, 2, 3));
            Assert.AreEqual(2, exports.Test(1, 2, 0));
            Assert.AreEqual(4, exports.Test(4, 5, 1));
            Assert.AreEqual(5, exports.Test(4, 5, 0));
        }
    }
}