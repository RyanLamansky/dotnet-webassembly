namespace WebAssembly;

/// <summary>
/// The packed storage type of a <see cref="StructField"/> (WASM 3.0 garbage collection).
/// The packed <c>i8</c>/<c>i16</c> storage kinds are not <see cref="WebAssemblyValueType"/> members, so a field
/// carries this discriminator alongside its <see cref="StructField.StorageType"/>.
/// </summary>
public enum PackedType : byte
{
    /// <summary>
    /// The field is not packed; its storage kind is the regular <see cref="StructField.StorageType"/> value.
    /// </summary>
    None,
    /// <summary>
    /// The field is packed to 8 bits (<c>i8</c>).  Encoded as <c>0x78</c>.
    /// </summary>
    I8,
    /// <summary>
    /// The field is packed to 16 bits (<c>i16</c>).  Encoded as <c>0x77</c>.
    /// </summary>
    I16,
}
