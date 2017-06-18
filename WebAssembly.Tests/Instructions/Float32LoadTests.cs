using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float32Load"/> instruction.
	/// </summary>
	[TestClass]
	public class Float32LoadTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float32Load"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32Load_Compiled_Offset0()
		{
			var compiled = MemoryReadTestBase<float>.CreateInstance(
				new GetLocal(),
				new Float32Load(),
				new End()
			);

			using (compiled)
			{
				Assert.IsNotNull(compiled);
				Assert.AreNotEqual(IntPtr.Zero, compiled.Start);
				Assert.AreNotEqual(IntPtr.Zero, compiled.End);

				var exports = compiled.Exports;
				Assert.AreEqual(0, exports.Test(0));

				var invariantCulture = CultureInfo.InvariantCulture;

				var testData = Samples.Memory;
				Marshal.Copy(testData, 0, compiled.Start, testData.Length);
				Assert.AreEqual("1.540035E-36", exports.Test(0).ToString(invariantCulture));
				Assert.AreEqual("6.207163E-36", exports.Test(1).ToString(invariantCulture));
				Assert.AreEqual("2.501747E-35", exports.Test(2).ToString(invariantCulture));
				Assert.AreEqual("1.008251E-34", exports.Test(3).ToString(invariantCulture));
				Assert.AreEqual("4.063216E-34", exports.Test(4).ToString(invariantCulture));
				Assert.AreEqual("0.03320982", exports.Test(5).ToString(invariantCulture));
				Assert.AreEqual("-8.313687E+14", exports.Test(6).ToString(invariantCulture));
				Assert.AreEqual("9.602914E-14", exports.Test(7).ToString(invariantCulture));
				Assert.AreEqual("-1.912281E+17", exports.Test(8).ToString(invariantCulture));

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
		/// Tests compilation and execution of the <see cref="Float32Load"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32Load_Compiled_Offset1()
		{
			var compiled = MemoryReadTestBase<float>.CreateInstance(
				new GetLocal(),
				new Float32Load
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

				var invariantCulture = CultureInfo.InvariantCulture;

				var testData = Samples.Memory;
				Marshal.Copy(testData, 0, compiled.Start, testData.Length);
				Assert.AreEqual("6.207163E-36", exports.Test(0).ToString(invariantCulture));
				Assert.AreEqual("2.501747E-35", exports.Test(1).ToString(invariantCulture));
				Assert.AreEqual("1.008251E-34", exports.Test(2).ToString(invariantCulture));
				Assert.AreEqual("4.063216E-34", exports.Test(3).ToString(invariantCulture));
				Assert.AreEqual("0.03320982", exports.Test(4).ToString(invariantCulture));
				Assert.AreEqual("-8.313687E+14", exports.Test(5).ToString(invariantCulture));
				Assert.AreEqual("9.602914E-14", exports.Test(6).ToString(invariantCulture));
				Assert.AreEqual("-1.912281E+17", exports.Test(7).ToString(invariantCulture));
				Assert.AreEqual("2.021882E-38", exports.Test(8).ToString(invariantCulture));

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