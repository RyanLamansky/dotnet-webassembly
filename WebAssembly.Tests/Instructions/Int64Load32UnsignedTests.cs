using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64Load32Unsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64Load32UnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load32Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Load32Unsigned_Compiled_Offset0()
        {
            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new GetLocal(),
                new Int64Load32Unsigned(),
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
                Assert.AreEqual(67306238, exports.Test(0));
                Assert.AreEqual(84148994, exports.Test(1));
                Assert.AreEqual(100992003, exports.Test(2));
                Assert.AreEqual(117835012, exports.Test(3));
                Assert.AreEqual(134678021, exports.Test(4));
                Assert.AreEqual(1023936262, exports.Test(5));
                Assert.AreEqual(3627878407, exports.Test(6));
                Assert.AreEqual(702037256, exports.Test(7));
                Assert.AreEqual(3693729853, exports.Test(8));

                Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 4));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 3));
                Assert.AreEqual(Memory.PageSize - 3, x.Offset);
                Assert.AreEqual(4u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2));
                Assert.AreEqual(Memory.PageSize - 2, x.Offset);
                Assert.AreEqual(4u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
                Assert.AreEqual(Memory.PageSize - 1, x.Offset);
                Assert.AreEqual(4u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(4u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
            }
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load32Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Load32Unsigned_Compiled_Offset1()
        {
            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new GetLocal(),
                new Int64Load32Unsigned
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
                Assert.AreEqual(84148994, exports.Test(0));
                Assert.AreEqual(100992003, exports.Test(1));
                Assert.AreEqual(117835012, exports.Test(2));
                Assert.AreEqual(134678021, exports.Test(3));
                Assert.AreEqual(1023936262, exports.Test(4));
                Assert.AreEqual(3627878407, exports.Test(5));
                Assert.AreEqual(702037256, exports.Test(6));
                Assert.AreEqual(3693729853, exports.Test(7));
                Assert.AreEqual(14428632, exports.Test(8));

                Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 5));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 4));
                Assert.AreEqual(Memory.PageSize - 3, x.Offset);
                Assert.AreEqual(4u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 3));
                Assert.AreEqual(Memory.PageSize - 2, x.Offset);
                Assert.AreEqual(4u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2));
                Assert.AreEqual(Memory.PageSize - 1, x.Offset);
                Assert.AreEqual(4u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(4u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
            }
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load8Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Load32Unsigned_Compiled_Then_Shift()
        {
            // Adapted from Int64Load8Unsigned_Compiled_Then_Shift.
            const int off = 4;
            const byte b = 0x9f;
            const int shift = 40;

            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new GetLocal(),
                new Int64Load32Unsigned
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

                Marshal.WriteByte(memory.Start + off, b);
                const long should_be = ((long)b) << shift;
                Assert.AreEqual(should_be, exports.Test(0));
            }
        }
    }
}