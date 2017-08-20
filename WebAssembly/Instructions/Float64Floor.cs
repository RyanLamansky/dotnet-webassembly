using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Floor operator.
	/// </summary>
	public class Float64Floor : ValueOneToOneCallInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Floor"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Floor;

		/// <summary>
		/// Creates a new  <see cref="Float64Floor"/> instance.
		/// </summary>
		public Float64Floor()
		{
		}

		internal sealed override MethodInfo MethodInfo => Method;

		internal sealed override ValueType ValueType => ValueType.Float64;

		internal static readonly RegeneratingWeakReference<MethodInfo> Method = new RegeneratingWeakReference<MethodInfo>(() =>
			typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
			{
				if (m.Name != nameof(Math.Floor))
					return false;

				var parms = m.GetParameters();
				return parms.Length == 1 && parms[0].ParameterType == typeof(double);
			}));
	}
}