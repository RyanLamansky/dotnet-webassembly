namespace WebAssembly
{
	/// <summary>
	/// Types suitable when a value is expected.
	/// </summary>
	public enum ValueType : sbyte
	{
		/// <summary>
		/// 32-bit integer value-type, equivalent to .NET's <see cref="int"/> and <see cref="uint"/>.
		/// </summary>
		Int32 = -0x01,
		/// <summary>
		/// 64-bit integer value-type, equivalent to .NET's <see cref="long"/> and <see cref="ulong"/>.
		/// </summary>
		Int64 = -0x02,
		/// <summary>
		/// 32-bit floating point value-type, equivalent to .NET's <see cref="float"/>.
		/// </summary>
		Float32 = -0x03,
		/// <summary>
		/// 64-bit floating point value-type, equivalent to .NET's <see cref="double"/>.
		/// </summary>
		Float64 = -0x04,
	}
}