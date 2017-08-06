using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float32Store"/> instruction.
	/// </summary>
	[TestClass]
	public class Float32StoreTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float32Store"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32Store_Compiled_Offset0()
		{
			var compiled = MemoryWriteTestBase<float>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Float32Store(),
				new End()
			);
			Assert.IsNotNull(compiled);

			using (compiled)
			{
				Assert.IsNotNull(compiled.Exports);
				var memory = compiled.Exports.Memory;
				Assert.AreNotEqual(IntPtr.Zero, memory.Start);

				var exports = compiled.Exports;
				exports.Test(0, (float)Math.PI);
				Assert.AreEqual(1078530011, Marshal.ReadInt32(memory.Start));
				Assert.AreEqual(4213007, Marshal.ReadInt32(memory.Start, 1));
				Assert.AreEqual(16457, Marshal.ReadInt32(memory.Start, 2));
				Assert.AreEqual(64, Marshal.ReadInt32(memory.Start, 3));

				exports.Test((int)Memory.PageSize - 4, 1);

				Assert.AreEqual(1065353216, Marshal.ReadInt32(memory.Start, (int)Memory.PageSize - 4));

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
		/// Tests compilation and execution of the <see cref="Float32Store"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32Store_Compiled_Offset1()
		{
			var compiled = MemoryWriteTestBase<float>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Float32Store() { Offset = 1 },
				new End()
			);
			Assert.IsNotNull(compiled);

			using (compiled)
			{
				Assert.IsNotNull(compiled.Exports);
				var memory = compiled.Exports.Memory;
				Assert.AreNotEqual(IntPtr.Zero, memory.Start);

				var exports = compiled.Exports;
				exports.Test(0, (float)Math.PI);
				Assert.AreEqual(1225775872, Marshal.ReadInt32(memory.Start));
				Assert.AreEqual(1078530011, Marshal.ReadInt32(memory.Start, 1));
				Assert.AreEqual(4213007, Marshal.ReadInt32(memory.Start, 2));
				Assert.AreEqual(16457, Marshal.ReadInt32(memory.Start, 3));
				Assert.AreEqual(64, Marshal.ReadInt32(memory.Start, 4));

				exports.Test((int)Memory.PageSize - 4 - 1, 1);

				Assert.AreEqual(1065353216, Marshal.ReadInt32(memory.Start, (int)Memory.PageSize - 4));

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