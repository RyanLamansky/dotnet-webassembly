using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests that empty standard sections (a declared section whose entry count is zero) are accepted as the
/// no-ops they are, rather than causing the parser to read past the section or demand absent prerequisites.
/// </summary>
[TestClass]
public class EmptySectionTests
{
    static void CompileAndInstantiate(params ReadOnlySpan<byte> sectionBytes)
    {
        using var stream = new MemoryStream([0x00, 0x61, 0x73, 0x6d, 0x01, 0x00, 0x00, 0x00, .. sectionBytes]);
        var maker = Compile.FromBinary<object>(stream);
        Assert.IsNotNull(maker(new ImportDictionary()));
    }

    /// <summary>An empty memory section (count 0) defines no memory and does not over-read.</summary>
    [TestMethod]
    public void EmptyMemorySection_Loads() => CompileAndInstantiate(0x05, 0x01, 0x00);

    /// <summary>An empty function section (count 0) loads even with no preceding type section.</summary>
    [TestMethod]
    public void EmptyFunctionSection_Loads() => CompileAndInstantiate(0x03, 0x01, 0x00);

    /// <summary>An empty code section (count 0) loads even with no preceding function section.</summary>
    [TestMethod]
    public void EmptyCodeSection_Loads() => CompileAndInstantiate(0x0a, 0x01, 0x00);
}
