using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64Load16Unsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64Load16UnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load16Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Load16Unsigned_Compiled_Offset0()
        {
            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new LocalGet(),
                new Int64Load16Unsigned(),
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
                Assert.AreEqual(766, exports.Test(0));
                Assert.AreEqual(770, exports.Test(1));
                Assert.AreEqual(1027, exports.Test(2));
                Assert.AreEqual(1284, exports.Test(3));
                Assert.AreEqual(1541, exports.Test(4));
                Assert.AreEqual(1798, exports.Test(5));
                Assert.AreEqual(2055, exports.Test(6));
                Assert.AreEqual(15624, exports.Test(7));
                Assert.AreEqual(55357, exports.Test(8));

                Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 2));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
                Assert.AreEqual(Memory.PageSize - 1, x.Offset);
                Assert.AreEqual(2u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(2u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
            }
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load16Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Load16Unsigned_Compiled_Offset1()
        {
            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new LocalGet(),
                new Int64Load16Unsigned
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
                Assert.AreEqual(770, exports.Test(0));
                Assert.AreEqual(1027, exports.Test(1));
                Assert.AreEqual(1284, exports.Test(2));
                Assert.AreEqual(1541, exports.Test(3));
                Assert.AreEqual(1798, exports.Test(4));
                Assert.AreEqual(2055, exports.Test(5));
                Assert.AreEqual(15624, exports.Test(6));
                Assert.AreEqual(55357, exports.Test(7));
                Assert.AreEqual(10712, exports.Test(8));

                Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 3));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2));
                Assert.AreEqual(Memory.PageSize - 1, x.Offset);
                Assert.AreEqual(2u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(2u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
            }
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64Load8Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64Load16Unsigned_Compiled_Then_Shift()
        {
            // Adapted from Int64Load8Unsigned_Compiled_Then_Shift.
            const int off = 4;
            const byte b = 0x9f;
            const int shift = 40;

            var compiled = MemoryReadTestBase<long>.CreateInstance(
                new LocalGet(),
                new Int64Load16Unsigned
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