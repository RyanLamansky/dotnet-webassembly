using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly
{
	static class TestUtility
	{
		/// <summary>
		/// Creates two instances of type <typeparamref name="T"/>, performs some basic tests, and returns them via out parameters.
		/// </summary>
		/// <typeparam name="T">The type to create.</typeparam>
		/// <param name="a">Receives the first instance.</param>
		/// <param name="b">Receives the second instance.</param>
		/// <exception cref="AssertFailedException">The hash codes do not match or the objects are not equal.</exception>
		public static void CreateInstances<T>(out T a, out T b)
			where T : class, new()
		{
			a = new T();
			b = new T();

			AreEqual(a, a);
			AreNotEqual(a, null);
			AreEqual(a, b);
		}

		/// <summary>
		/// Checks <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> on both parameters.
		/// </summary>
		/// <param name="a">The first item to check.</param>
		/// <param name="b">The second item to check.</param>
		/// <exception cref="AssertFailedException">The hash codes do not match or the objects are not equal.</exception>
		public static void AreEqual(object a, object b)
		{
			Assert.AreEqual(a?.GetHashCode(), b?.GetHashCode());
			Assert.IsTrue((a?.Equals(b)).GetValueOrDefault());
			if (a != null)
				Assert.IsNotNull(a.ToString());
			if (b != null)
				Assert.IsNotNull(b.ToString());
		}

		/// <summary>
		/// Checks <see cref="object.Equals(object)"/> on the provided parameters.
		/// </summary>
		/// <param name="a">The first item to check.</param>
		/// <param name="b">The second item to check.</param>
		/// <exception cref="AssertFailedException">The objects are not equal.</exception>
		public static void AreNotEqual(object a, object b)
		{
			Assert.IsFalse((a?.Equals(b)).GetValueOrDefault());
			if (a != null)
				Assert.IsNotNull(a.ToString());
			if (b != null)
				Assert.IsNotNull(b.ToString());
		}
	}
}