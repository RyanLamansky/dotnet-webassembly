using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64Load8Signed"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64Load8SignedSignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load8Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32Load8Signed_Compiled_Offset0()
        {
            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new GetLocal(),
                new Int64Load8Signed(),
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

                var testData = Samples.Memory;
                Marshal.Copy(testData, 0, memory.Start, testData.Length);
                Assert.AreEqual(-2, exports.Test(0));
                Assert.AreEqual(2, exports.Test(1));
                Assert.AreEqual(3, exports.Test(2));
                Assert.AreEqual(4, exports.Test(3));
                Assert.AreEqual(5, exports.Test(4));
                Assert.AreEqual(6, exports.Test(5));
                Assert.AreEqual(7, exports.Test(6));
                Assert.AreEqual(8, exports.Test(7));
                Assert.AreEqual(61, exports.Test(8));

                Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 1));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(1u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
            }
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load8Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Load8Signed_Compiled_Offset1()
        {
            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new GetLocal(),
                new Int64Load8Signed
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

                var testData = Samples.Memory;
                Marshal.Copy(testData, 0, memory.Start, testData.Length);
                Assert.AreEqual(2, exports.Test(0));
                Assert.AreEqual(3, exports.Test(1));
                Assert.AreEqual(4, exports.Test(2));
                Assert.AreEqual(5, exports.Test(3));
                Assert.AreEqual(6, exports.Test(4));
                Assert.AreEqual(7, exports.Test(5));
                Assert.AreEqual(8, exports.Test(6));
                Assert.AreEqual(61, exports.Test(7));
                Assert.AreEqual(-40, exports.Test(8));

                Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 2));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(1u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
            }
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load8Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Load8Signed_Compiled_Then_Shift()
        {
            // Adapted from Int64Load8Unsigned_Compiled_Then_Shift.
            const int off = 4;
            const sbyte b = 0x5f;
            const int shift = 40;

            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new GetLocal(),
                new Int64Load8Signed
                {
                    Offset = off,
                },
                new Int64Constant(shift),
                new Int64ShiftLeft(),
                new End()
            );

            using (compiled)
            {
                Assert.IsNotNull(compiled);
                Assert.IsNotNull(compiled.Exports);
                var memory = compiled.Exports.Memory;
                Assert.AreNotEqual(IntPtr.Zero, memory.Start);

                var exports = compiled.Exports;

                Marshal.WriteByte(memory.Start + off, (byte)b);
                const long should_be = ((long)b) << shift;
                Assert.AreEqual(should_be, exports.Test(0));
            }
        }
    }
}