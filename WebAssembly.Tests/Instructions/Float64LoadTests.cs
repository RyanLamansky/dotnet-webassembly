using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Load"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64LoadTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Load"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Load_Compiled_Offset0()
        {
            var compiled = MemoryReadTestBase<double>.CreateInstance(
                new GetLocal(),
                new Float64Load(),
                new End()
            );

            using (compiled)
            {
                Assert.IsNotNull(compiled);
                Assert.IsNotNull(compiled.Exports);
                var memory = compiled.Exports.Memory;
                Assert.AreNotEqual(IntPtr.Zero, memory.Start);

                var exports = compiled.Exports;
                Assert.AreEqual(0, exports.Test(0));

                var invariantCulture = CultureInfo.InvariantCulture;

                var testData = Samples.Memory;
                Marshal.Copy(testData, 0, memory.Start, testData.Length);
                Assert.AreEqual("5.44760372201182E-270", exports.Test(0).ToString(invariantCulture));
                Assert.AreEqual("1.06703248910785E-14", exports.Test(1).ToString(invariantCulture));
                Assert.AreEqual("-1.14389371511465E+117", exports.Test(2).ToString(invariantCulture));
                Assert.AreEqual("4.12824598825351E-107", exports.Test(3).ToString(invariantCulture));
                Assert.AreEqual("-9.39245758009613E+135", exports.Test(4).ToString(invariantCulture));
                Assert.AreEqual("1.60424369241791E-304", exports.Test(5).ToString(invariantCulture));
                Assert.AreEqual("1.19599597184682E-309", exports.Test(6).ToString(invariantCulture));
                Assert.AreEqual("4.6718592650265E-312", exports.Test(7).ToString(invariantCulture));
                Assert.AreEqual("1.82494502538554E-314", exports.Test(8).ToString(invariantCulture));

                Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 8));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 7));
                Assert.AreEqual(Memory.PageSize - 7, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 6));
                Assert.AreEqual(Memory.PageSize - 6, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
                Assert.AreEqual(Memory.PageSize - 1, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(8u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
            }
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Load"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Load_Compiled_Offset1()
        {
            var compiled = MemoryReadTestBase<double>.CreateInstance(
                new GetLocal(),
                new Float64Load
                {
                    Offset = 1,
                },
                new End()
            );

            using (compiled)
            {
                Assert.IsNotNull(compiled);
                Assert.IsNotNull(compiled.Exports);
                var memory = compiled.Exports.Memory;
                Assert.AreNotEqual(IntPtr.Zero, memory.Start);

                var exports = compiled.Exports;
                Assert.AreEqual(0, exports.Test(0));

                var invariantCulture = CultureInfo.InvariantCulture;

                var testData = Samples.Memory;
                Marshal.Copy(testData, 0, memory.Start, testData.Length);
                Assert.AreEqual("1.06703248910785E-14", exports.Test(0).ToString(invariantCulture));
                Assert.AreEqual("-1.14389371511465E+117", exports.Test(1).ToString(invariantCulture));
                Assert.AreEqual("4.12824598825351E-107", exports.Test(2).ToString(invariantCulture));
                Assert.AreEqual("-9.39245758009613E+135", exports.Test(3).ToString(invariantCulture));
                Assert.AreEqual("1.60424369241791E-304", exports.Test(4).ToString(invariantCulture));
                Assert.AreEqual("1.19599597184682E-309", exports.Test(5).ToString(invariantCulture));
                Assert.AreEqual("4.6718592650265E-312", exports.Test(6).ToString(invariantCulture));
                Assert.AreEqual("1.82494502538554E-314", exports.Test(7).ToString(invariantCulture));
                Assert.AreEqual("7.12869138768568E-317", exports.Test(8).ToString(invariantCulture));

                Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 9));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 8));
                Assert.AreEqual(Memory.PageSize - 7, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 6));
                Assert.AreEqual(Memory.PageSize - 5, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2));
                Assert.AreEqual(Memory.PageSize - 1, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(8u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
            }
        }
    }
}