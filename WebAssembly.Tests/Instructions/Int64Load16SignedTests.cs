using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64Load16Signed"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64Load16SignedTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Load16Signed"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Load16Signed_Compiled_Offset0()
		{
			var compiled = MemoryReadTestBase<long>.CreateInstance(
				new GetLocal(),
				new Int64Load16Signed(),
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
				Assert.AreEqual(766, exports.Test(0));
				Assert.AreEqual(770, exports.Test(1));
				Assert.AreEqual(1027, exports.Test(2));
				Assert.AreEqual(1284, exports.Test(3));
				Assert.AreEqual(1541, exports.Test(4));
				Assert.AreEqual(1798, exports.Test(5));
				Assert.AreEqual(2055, exports.Test(6));
				Assert.AreEqual(15624, exports.Test(7));
				Assert.AreEqual(-10179, exports.Test(8));

				Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 2));

				MemoryAccessOutOfRangeException x;

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
				Assert.AreEqual(Memory.PageSize - 1, x.Offset);
				Assert.AreEqual(2u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize));
				Assert.AreEqual(Memory.PageSize, x.Offset);
				Assert.AreEqual(2u, x.Length);

				ExceptionAssert.Expect<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
			}

			Assert.AreEqual(IntPtr.Zero, compiled.Start);
			Assert.AreEqual(IntPtr.Zero, compiled.End);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Load16Signed"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Load16Signed_Compiled_Offset1()
		{
			var compiled = MemoryReadTestBase<long>.CreateInstance(
				new GetLocal(),
				new Int64Load16Signed
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
				Assert.AreEqual(770, exports.Test(0));
				Assert.AreEqual(1027, exports.Test(1));
				Assert.AreEqual(1284, exports.Test(2));
				Assert.AreEqual(1541, exports.Test(3));
				Assert.AreEqual(1798, exports.Test(4));
				Assert.AreEqual(2055, exports.Test(5));
				Assert.AreEqual(15624, exports.Test(6));
				Assert.AreEqual(-10179, exports.Test(7));
				Assert.AreEqual(10712, exports.Test(8));

				Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 3));

				MemoryAccessOutOfRangeException x;

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2));
				Assert.AreEqual(Memory.PageSize - 1, x.Offset);
				Assert.AreEqual(2u, x.Length);

				x = ExceptionAssert.Expect<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
				Assert.AreEqual(Memory.PageSize, x.Offset);
				Assert.AreEqual(2u, x.Length);

				ExceptionAssert.Expect<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
			}

			Assert.AreEqual(IntPtr.Zero, compiled.Start);
			Assert.AreEqual(IntPtr.Zero, compiled.End);
		}
	}
}