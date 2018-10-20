using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64Load"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64LoadTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Load"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Load_Compiled_Offset0()
		{
			var compiled = MemoryReadTestBase<long>.CreateInstance(
				new GetLocal(),
				new Int64Load(),
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
				Assert.AreEqual(578437695752307454, exports.Test(0));
				Assert.AreEqual(4397772758562636546, exports.Test(1));
				Assert.AreEqual(-2865124961678982141, exports.Test(2));
				Assert.AreEqual(3015227055211414788, exports.Test(3));
				Assert.AreEqual(-2582295154680986107, exports.Test(4));
				Assert.AreEqual(61970503589955334, exports.Test(5));
				Assert.AreEqual(242072279648263, exports.Test(6));
				Assert.AreEqual(945594842376, exports.Test(7));
				Assert.AreEqual(3693729853, exports.Test(8));

				Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 8));

				MemoryAccessOutOfRangeException x;

				x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 7));
				Assert.AreEqual(Memory.PageSize - 7, x.Offset);
				Assert.AreEqual(8u, x.Length);

				x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 6));
				Assert.AreEqual(Memory.PageSize - 6, x.Offset);
				Assert.AreEqual(8u, x.Length);

				x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
				Assert.AreEqual(Memory.PageSize - 1, x.Offset);
				Assert.AreEqual(8u, x.Length);

				x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize));
				Assert.AreEqual(Memory.PageSize, x.Offset);
				Assert.AreEqual(8u, x.Length);

				Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
			}
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Load"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Load_Compiled_Offset1()
		{
			var compiled = MemoryReadTestBase<long>.CreateInstance(
				new GetLocal(),
				new Int64Load
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
				Assert.AreEqual(4397772758562636546, exports.Test(0));
				Assert.AreEqual(-2865124961678982141, exports.Test(1));
				Assert.AreEqual(3015227055211414788, exports.Test(2));
				Assert.AreEqual(-2582295154680986107, exports.Test(3));
				Assert.AreEqual(61970503589955334, exports.Test(4));
				Assert.AreEqual(242072279648263, exports.Test(5));
				Assert.AreEqual(945594842376, exports.Test(6));
				Assert.AreEqual(3693729853, exports.Test(7));
				Assert.AreEqual(14428632, exports.Test(8));

				Assert.AreEqual(0, exports.Test((int)Memory.PageSize - 9));

				MemoryAccessOutOfRangeException x;

				x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 8));
				Assert.AreEqual(Memory.PageSize - 7, x.Offset);
				Assert.AreEqual(8u, x.Length);

				x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 6));
				Assert.AreEqual(Memory.PageSize - 5, x.Offset);
				Assert.AreEqual(8u, x.Length);

				x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 2));
				Assert.AreEqual(Memory.PageSize - 1, x.Offset);
				Assert.AreEqual(8u, x.Length);

				x = Assert.ThrowsException<MemoryAccessOutOfRangeException>(() => exports.Test((int)Memory.PageSize - 1));
				Assert.AreEqual(Memory.PageSize, x.Offset);
				Assert.AreEqual(8u, x.Length);

				Assert.ThrowsException<OverflowException>(() => exports.Test(unchecked((int)uint.MaxValue)));
			}
		}
	}
}