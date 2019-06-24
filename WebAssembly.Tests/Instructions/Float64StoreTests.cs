using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64Store"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64StoreTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Store"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Store_Compiled_Offset0()
        {
            var compiled = MemoryWriteTestBase<double>.CreateInstance(
                new GetLocal(0),
                new GetLocal(1),
                new Float64Store(),
                new End()
            );
            Assert.IsNotNull(compiled);

            using (compiled)
            {
                Assert.IsNotNull(compiled.Exports);
                var memory = compiled.Exports.Memory;
                Assert.AreNotEqual(IntPtr.Zero, memory.Start);

                var exports = compiled.Exports;
                exports.Test(0, Math.PI);
                Assert.AreEqual(1413754136, Marshal.ReadInt32(memory.Start));
                Assert.AreEqual(-78363603, Marshal.ReadInt32(memory.Start, 1));
                Assert.AreEqual(570119236, Marshal.ReadInt32(memory.Start, 2));
                Assert.AreEqual(153221972, Marshal.ReadInt32(memory.Start, 3));

                exports.Test((int)Memory.PageSize - 8, 1);

                Assert.AreEqual(4607182418800017408, Marshal.ReadInt64(memory.Start, (int)Memory.PageSize - 8));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 7, 0));
                Assert.AreEqual(Memory.PageSize - 7, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 6, 0));
                Assert.AreEqual(Memory.PageSize - 6, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 5, 0));
                Assert.AreEqual(Memory.PageSize - 5, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 4, 0));
                Assert.AreEqual(Memory.PageSize - 4, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 3, 0));
                Assert.AreEqual(Memory.PageSize - 3, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2, 0));
                Assert.AreEqual(Memory.PageSize - 2, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1, 0));
                Assert.AreEqual(Memory.PageSize - 1, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize, 0));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(8u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue), 0));
            }
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64Store"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64Store_Compiled_Offset1()
        {
            var compiled = MemoryWriteTestBase<double>.CreateInstance(
                new GetLocal(0),
                new GetLocal(1),
                new Float64Store() { Offset = 1 },
                new End()
            );
            Assert.IsNotNull(compiled);

            using (compiled)
            {
                Assert.IsNotNull(compiled.Exports);
                var memory = compiled.Exports.Memory;
                Assert.AreNotEqual(IntPtr.Zero, memory.Start);

                var exports = compiled.Exports;
                exports.Test(0, Math.PI);
                Assert.AreEqual(1143805952, Marshal.ReadInt32(memory.Start));
                Assert.AreEqual(1413754136, Marshal.ReadInt32(memory.Start, 1));
                Assert.AreEqual(-78363603, Marshal.ReadInt32(memory.Start, 2));
                Assert.AreEqual(570119236, Marshal.ReadInt32(memory.Start, 3));
                Assert.AreEqual(153221972, Marshal.ReadInt32(memory.Start, 4));

                exports.Test((int)Memory.PageSize - 8 - 1, 1);

                Assert.AreEqual(4607182418800017408, Marshal.ReadInt64(memory.Start, (int)Memory.PageSize - 8));

                MemoryAccessOutOfRangeException x;

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 8, 0));
                Assert.AreEqual(Memory.PageSize - 7, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 7, 0));
                Assert.AreEqual(Memory.PageSize - 6, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 6, 0));
                Assert.AreEqual(Memory.PageSize - 5, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 5, 0));
                Assert.AreEqual(Memory.PageSize - 4, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 4, 0));
                Assert.AreEqual(Memory.PageSize - 3, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 3, 0));
                Assert.AreEqual(Memory.PageSize - 2, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2, 0));
                Assert.AreEqual(Memory.PageSize - 1, x.Offset);
                Assert.AreEqual(8u, x.Length);

                x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1, 0));
                Assert.AreEqual(Memory.PageSize, x.Offset);
                Assert.AreEqual(8u, x.Length);

                Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue), 0));
            }
        }
    }
}