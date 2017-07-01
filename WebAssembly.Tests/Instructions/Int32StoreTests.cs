using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int32Store"/> instruction.
	/// </summary>
	[TestClass]
	public class Int32StoreTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32Store"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32Store_Compiled_Offset0()
		{
			var compiled = MemoryWriteTestBase<int>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Int32Store(),
				new End()
			);
			Assert.IsNotNull(compiled);

			using (compiled)
			{
				Assert.AreNotEqual(IntPtr.Zero, compiled.Start);
				Assert.AreNotEqual(IntPtr.Zero, compiled.End);

				var exports = compiled.Exports;
				exports.Test(0, unchecked((int)2147483648));
				Assert.AreEqual(-2147483648, Marshal.ReadInt32(compiled.Start));
				Assert.AreEqual(8388608, Marshal.ReadInt32(compiled.Start, 1));
				Assert.AreEqual(32768, Marshal.ReadInt32(compiled.Start, 2));
				Assert.AreEqual(128, Marshal.ReadInt32(compiled.Start, 3));

				exports.Test((int)Memory.PageSize - 4, 1);

				Assert.AreEqual(1, Marshal.ReadInt32(compiled.Start, (int)Memory.PageSize - 4));

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

			Assert.AreEqual(IntPtr.Zero, compiled.Start);
			Assert.AreEqual(IntPtr.Zero, compiled.End);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Int32Store"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int32Store_Compiled_Offset1()
		{
			var compiled = MemoryWriteTestBase<int>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Int32Store() { Offset = 1 },
				new End()
			);
			Assert.IsNotNull(compiled);

			using (compiled)
			{
				Assert.AreNotEqual(IntPtr.Zero, compiled.Start);
				Assert.AreNotEqual(IntPtr.Zero, compiled.End);

				var exports = compiled.Exports;
				exports.Test(0, unchecked((int)2147483648));
				Assert.AreEqual(0, Marshal.ReadInt32(compiled.Start));
				Assert.AreEqual(-2147483648, Marshal.ReadInt32(compiled.Start, 1));
				Assert.AreEqual(8388608, Marshal.ReadInt32(compiled.Start, 2));
				Assert.AreEqual(32768, Marshal.ReadInt32(compiled.Start, 3));
				Assert.AreEqual(128, Marshal.ReadInt32(compiled.Start, 4));

				exports.Test((int)Memory.PageSize - 4 - 1, 1);

				Assert.AreEqual(1, Marshal.ReadInt32(compiled.Start, (int)Memory.PageSize - 4));

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

			Assert.AreEqual(IntPtr.Zero, compiled.Start);
			Assert.AreEqual(IntPtr.Zero, compiled.End);
		}
	}
}