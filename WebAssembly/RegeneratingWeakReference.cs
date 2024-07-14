using System;

namespace WebAssembly;

readonly struct RegeneratingWeakReference<T>(Func<T> regenerator)
    where T : class
{
    private readonly WeakReference<T> reference = new(null!);

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
