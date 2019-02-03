namespace WebAssembly
{
    /// <summary>
    /// Applied to <see cref="Type"/> to indicate the type of function.
    /// </summary>
    public enum FunctionType : sbyte
    {
        /// <summary>
        /// A function.
        /// </summary>
        Function = -0x20,
    }
}