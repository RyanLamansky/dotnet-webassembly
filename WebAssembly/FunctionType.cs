namespace WebAssembly
{
    /// <summary>
    /// Applied to <see cref="WebAssemblyType"/> to indicate the type of function.
    /// </summary>
    public enum FunctionType : sbyte
    {
        /// <summary>
        /// A function.
        /// </summary>
        Function = -0x20,
    }
}