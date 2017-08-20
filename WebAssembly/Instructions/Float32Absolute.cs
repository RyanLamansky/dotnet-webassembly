using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Absolute value.
	/// </summary>
	public class Float32Absolute : ValueOneToOneCallInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Absolute"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Absolute;

		/// <summary>
		/// Creates a new  <see cref="Float32Absolute"/> instance.
		/// </summary>
		public Float32Absolute()
		{
		}

		internal sealed override MethodInfo MethodInfo => method;

		internal sealed override ValueType ValueType => ValueType.Float32;

		private static readonly RegeneratingWeakReference<MethodInfo> method = new RegeneratingWeakReference<MethodInfo>(() =>
			typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
			{
				if (m.Name != nameof(Math.Abs))
					return false;

				var parms = m.GetParameters();
				return parms.Length == 1 && parms[0].ParameterType == typeof(float);
			}));
	}
}