using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

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
				Assert.AreNotEqual(IntPtr.Zero, compiled.Start);
				Assert.AreNotEqual(IntPtr.Zero, compiled.End);

				var exports = compiled.Exports;
				Assert.AreEqual(0, exports.Test(0));

				var testData = Samples.Memory;
				Marshal.Copy(testData, 0, compiled.Start, testData.Length);
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

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 3));
				Assert.AreEqual(Memory.PageSize - 3, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2));
				Assert.AreEqual(Memory.PageSize - 2, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
				Assert.AreEqual(Memory.PageSize - 1, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize));
				Assert.AreEqual(Memory.PageSize, x.Offset);
				Assert.AreEqual(4u, x.Length);

				ExceptionAssert.Expect<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
			}

			Assert.AreEqual(IntPtr.Zero, compiled.Start);
			Assert.AreEqual(IntPtr.Zero, compiled.End);
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
				Assert.AreNotEqual(IntPtr.Zero, compiled.Start);
				Assert.AreNotEqual(IntPtr.Zero, compiled.End);

				var exports = compiled.Exports;
				Assert.AreEqual(0, exports.Test(0));

				var testData = Samples.Memory;
				Marshal.Copy(testData, 0, compiled.Start, testData.Length);
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

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 4));
				Assert.AreEqual(Memory.PageSize - 3, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 3));
				Assert.AreEqual(Memory.PageSize - 2, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2));
				Assert.AreEqual(Memory.PageSize - 1, x.Offset);
				Assert.AreEqual(4u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
				Assert.AreEqual(Memory.PageSize, x.Offset);
				Assert.AreEqual(4u, x.Length);

				ExceptionAssert.Expect<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
			}

			Assert.AreEqual(IntPtr.Zero, compiled.Start);
			Assert.AreEqual(IntPtr.Zero, compiled.End);
		}
	}
}