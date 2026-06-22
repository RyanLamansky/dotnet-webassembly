using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests <see cref="MissingDelegateTypesException"/> and <see cref="MissingDelegateType"/>, raised when
/// <see cref="CompilerConfiguration.GetDelegateForType"/> cannot supply a delegate type for an import.
/// </summary>
[TestClass]
public class MissingDelegateTypesExceptionTests
{
    /// <summary>An export surface with no members; the imports are what matter here.</summary>
    public abstract class Empty
    {
    }

    private static MissingDelegateTypesException MissesFor(params WebAssemblyType[] importedTypes)
    {
        var module = new Module();
        for (var i = 0; i < importedTypes.Length; i++)
        {
            module.Types.Add(importedTypes[i]);
            module.Imports.Add(new Import.Function { Module = "mod", Field = $"f{i}", TypeIndex = (uint)i });
        }

        using var memory = new MemoryStream();
        module.WriteToBinary(memory);
        memory.Position = 0;

        // A configuration that never provides a delegate type forces every import to be reported as missing.
        var configuration = new CompilerConfiguration { GetDelegateForType = (parameters, returns) => null };
        return Assert.ThrowsExactly<MissingDelegateTypesException>(() => Compile.FromBinary<Empty>(memory, configuration));
    }

    /// <summary>
    /// Each import that cannot be matched to a delegate type appears once in
    /// <see cref="MissingDelegateTypesException.MissingDelegateTypes"/> with its module, field, and arities.
    /// </summary>
    [TestMethod]
    public void MissingDelegateTypes_CollectsEveryMiss()
    {
        var exception = MissesFor(
            new WebAssemblyType { Parameters = [WebAssemblyValueType.Float64, WebAssemblyValueType.Float64], Returns = [WebAssemblyValueType.Float64] },
            new WebAssemblyType { Parameters = [], Returns = [] });

        Assert.AreEqual(2, exception.MissingDelegateTypes.Count);

        var first = exception.MissingDelegateTypes.Single(m => m.Field == "f0");
        Assert.AreEqual("mod", first.Module);
        Assert.AreEqual(2, first.Parameters);
        Assert.AreEqual(1, first.Returns);

        var second = exception.MissingDelegateTypes.Single(m => m.Field == "f1");
        Assert.AreEqual(0, second.Parameters);
        Assert.AreEqual(0, second.Returns);

        // The message names every miss.
        StringAssert.Contains(exception.Message, first.ToString());
        StringAssert.Contains(exception.Message, second.ToString());
    }

    private static string DescribeMiss(WebAssemblyType type)
        => MissesFor(type).MissingDelegateTypes.Single().ToString();

    /// <summary>
    /// <see cref="MissingDelegateType.ToString"/> renders parameter and return counts using natural-language
    /// pluralization across all of its branches.
    /// </summary>
    [TestMethod]
    public void MissingDelegateType_ToString_FormatsArities()
    {
        // Singular parameter, zero returns.
        Assert.AreEqual(
            "1 parameter and no returns for mod::f0",
            DescribeMiss(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [] }));

        // Plural parameters, single return.
        Assert.AreEqual(
            "2 parameters and one return for mod::f0",
            DescribeMiss(new WebAssemblyType { Parameters = [WebAssemblyValueType.Float64, WebAssemblyValueType.Float64], Returns = [WebAssemblyValueType.Float64] }));

        // Zero parameters (also plural), multiple returns exercises the default branch.
        Assert.AreEqual(
            "0 parameters and 2 returns for mod::f0",
            DescribeMiss(new WebAssemblyType { Parameters = [], Returns = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32] }));
    }
}
