using System;

namespace WebAssembly
{
    struct RegeneratingWeakReference<T>
		where T : class
    {
		private readonly WeakReference<T> reference;
		private readonly Func<T> regenerator;

		public RegeneratingWeakReference(Func<T> regenerator)
		{
			this.reference = new WeakReference<T>(regenerator(), false);
			this.regenerator = regenerator;
		}

		public T Reference
		{
			get
			{
				if (!this.reference.TryGetTarget(out var value))
					this.reference.SetTarget(value = regenerator());

				return value;
			}
		}

		public static implicit operator T(RegeneratingWeakReference<T> reference) => reference.Reference;
    }
}