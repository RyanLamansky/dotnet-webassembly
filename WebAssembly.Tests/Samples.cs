namespace WebAssembly
{
	/// <summary>
	/// Provides diverse sample data values.
	/// </summary>
	static class Samples
	{
		public static int[] Int32 => new int[]
		{
			-1,
			0,
			1,
			0x00,
			0x0F,
			0xF0,
			0xFF,
			byte.MaxValue,
			short.MinValue,
			short.MaxValue,
			ushort.MaxValue,
			int.MinValue,
			int.MaxValue,
		};

		public static uint[] UInt32 => new uint[]
		{
			0,
			1,
			0x00,
			0x0F,
			0xF0,
			0xFF,
			byte.MaxValue,
			ushort.MaxValue,
			int.MaxValue,
			uint.MaxValue,
		};

		public static long[] Int64 => new long[]
		{
			-1,
			0,
			1,
			0x00,
			0x0F,
			0xF0,
			0xFF,
			byte.MaxValue,
			short.MinValue,
			short.MaxValue,
			ushort.MaxValue,
			int.MinValue,
			int.MaxValue,
			uint.MaxValue,
			long.MinValue,
			long.MaxValue,
		};

		public static ulong[] UInt64 => new ulong[]
		{
			0,
			1,
			0x00,
			0x0F,
			0xF0,
			0xFF,
			byte.MaxValue,
			ushort.MaxValue,
			int.MaxValue,
			uint.MaxValue,
			long.MaxValue,
			ulong.MaxValue,
		};
	}
}