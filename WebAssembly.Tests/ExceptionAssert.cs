using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly
{
	/// <summary>
	/// Validates that exception behaviors.
	/// </summary>
	static class ExceptionAssert
	{
		/// <summary>
		/// Ensures that a particular exception is returned with a populated <see cref="Exception.Message"/> property.
		/// </summary>
		/// <typeparam name="T">The exception type expected.  Inheritors of this type are also accepted.</typeparam>
		/// <param name="action">The action expected to trigger the exception.</param>
		/// <returns>The exception encountered.</returns>
		/// <exception cref="AssertFailedException">Expected exception was not encountered.</exception>
		public static T Expect<T>(Action action) where T : Exception
		{
			try
			{
				action();
			}
			catch (T x)
			{
				Assert.IsNotNull(x.Message, "Exception.Message is null.");
				return x;
			}
			catch (Exception x)
			{
				throw new AssertFailedException($"Expected an exception of type {typeof(T).FullName}, but received {x.GetType().FullName} instead.");
			}

			throw new AssertFailedException($"Expected an exception of type {typeof(T).FullName}, but nothing happened.");
		}
	}
}