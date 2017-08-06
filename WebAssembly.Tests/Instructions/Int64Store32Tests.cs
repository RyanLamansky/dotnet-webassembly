using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64Store32"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64Store32Tests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Store32"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Store32_Compiled_Offset0()
		{
			var compiled = MemoryWriteTestBase<long>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Int64Store32(),
				new End()
			);
			Assert.IsNotNull(compiled);

			using (compiled)
			{
				Assert.IsNotNull(compiled.Exports);
				var memory = compiled.Exports.Memory;
				Assert.AreNotEqual(IntPtr.Zero, memory.Start);

				var exports = compiled.Exports;
				exports.Test(0, 2147483648);
				Assert.AreEqual(-2147483648, Marshal.ReadInt32(memory.Start));
				Assert.AreEqual(8388608, Marshal.ReadInt32(memory.Start, 1));
				Assert.AreEqual(32768, Marshal.ReadInt32(memory.Start, 2));
				Assert.AreEqual(128, Marshal.ReadInt32(memory.Start, 3));
				Assert.AreEqual(0, Marshal.ReadInt32(memory.Start, 4));

				exports.Test((int)Memory.PageSize - 8, 1);

				Assert.AreEqual(1, Marshal.ReadInt64(memory.Start, (int)Memory.PageSize - 8));

				MemoryAccessOutOfRangeException x;

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 3, 0));
				Assert.AreEqual(Memory.PageSize - 3, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2, 0));
				Assert.AreEqual(Memory.PageSize - 2, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1, 0));
				Assert.AreEqual(Memory.PageSize - 1, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize, 0));
				Assert.AreEqual(Memory.PageSize, x.Offset);
				Assert.AreEqual(4u, x.Length);

				ExceptionAssert.Expect<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue), 0));
			}
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Store32"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Store32_Compiled_Offset1()
		{
			var compiled = MemoryWriteTestBase<long>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Int64Store32() { Offset = 1 },
				new End()
			);
			Assert.IsNotNull(compiled);

			using (compiled)
			{
				Assert.IsNotNull(compiled.Exports);
				var memory = compiled.Exports.Memory;
				Assert.AreNotEqual(IntPtr.Zero, memory.Start);

				var exports = compiled.Exports;
				exports.Test(0, 2147483648);
				Assert.AreEqual(0, Marshal.ReadInt32(memory.Start));
				Assert.AreEqual(-2147483648, Marshal.ReadInt32(memory.Start, 1));
				Assert.AreEqual(8388608, Marshal.ReadInt32(memory.Start, 2));
				Assert.AreEqual(32768, Marshal.ReadInt32(memory.Start, 3));
				Assert.AreEqual(128, Marshal.ReadInt32(memory.Start, 4));
				Assert.AreEqual(0, Marshal.ReadInt32(memory.Start, 5));

				exports.Test((int)Memory.PageSize - 8 - 1, 1);

				Assert.AreEqual(1, Marshal.ReadInt64(memory.Start, (int)Memory.PageSize - 8));

				MemoryAccessOutOfRangeException x;

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 4, 0));
				Assert.AreEqual(Memory.PageSize - 3, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 3, 0));
				Assert.AreEqual(Memory.PageSize - 2, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2, 0));
				Assert.AreEqual(Memory.PageSize - 1, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1, 0));
				Assert.AreEqual(Memory.PageSize, x.Offset);
				Assert.AreEqual(4u, x.Length);

				ExceptionAssert.Expect<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue), 0));
			}
		}
	}
}