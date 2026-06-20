using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// Tests the binary round-trip of <see cref="Element"/> segments across every <see cref="ElementKind"/>, exercising
/// the writer path for the non-active forms (the spec suite only reads binaries, never re-serializes them).
/// </summary>
[TestClass]
public class ElementTests
{
    private static Element RoundTrip(Element element)
    {
        var module = new Module();
        module.Elements.Add(element);

        using var memory = new MemoryStream();
        module.WriteToBinary(memory);
        memory.Position = 0;
        return Module.ReadFromBinary(memory).Elements.Single();
    }

    /// <summary>
    /// Verifies that each <see cref="ElementKind"/> survives a write/read round-trip with its kind, derived flags,
    /// and payload intact.
    /// </summary>
    [TestMethod]
    public void Element_RoundTrip_AllKinds()
    {
        // Active, implicit table 0, function indices.
        var active = RoundTrip(new Element(3, 1, 2));
        Assert.AreEqual(ElementKind.ActiveFunctionIndices, active.Kind);
        Assert.IsTrue(active.IsActive);
        Assert.IsFalse(active.UsesExpressions);
        CollectionAssert.AreEqual(new uint[] { 1, 2 }, active.Elements.ToArray());

        // Passive, function indices.
        var passive = RoundTrip(new Element { Kind = ElementKind.PassiveFunctionIndices, Elements = [4, 5, 6] });
        Assert.AreEqual(ElementKind.PassiveFunctionIndices, passive.Kind);
        Assert.IsFalse(passive.IsActive);
        Assert.IsFalse(passive.IsDeclarative);
        CollectionAssert.AreEqual(new uint[] { 4, 5, 6 }, passive.Elements.ToArray());

        // Active, explicit table index, function indices.
        var activeExplicit = RoundTrip(new Element
        {
            Kind = ElementKind.ActiveExplicitTableFunctionIndices,
            Index = 7,
            InitializerExpression = [new Int32Constant(0), new End()],
            Elements = [8],
        });
        Assert.AreEqual(ElementKind.ActiveExplicitTableFunctionIndices, activeExplicit.Kind);
        Assert.IsTrue(activeExplicit.IsActive);
        Assert.AreEqual(7u, activeExplicit.Index);
        CollectionAssert.AreEqual(new uint[] { 8 }, activeExplicit.Elements.ToArray());

        // Declarative, function indices.
        var declarative = RoundTrip(new Element { Kind = ElementKind.DeclarativeFunctionIndices, Elements = [9] });
        Assert.AreEqual(ElementKind.DeclarativeFunctionIndices, declarative.Kind);
        Assert.IsFalse(declarative.IsActive);
        Assert.IsTrue(declarative.IsDeclarative);

        // Passive, initializer expressions, externref.
        var passiveExprs = RoundTrip(new Element
        {
            Kind = ElementKind.PassiveExpressions,
            ElemType = ElementType.ExternRef,
            InitExprs = [[new RefNull(WebAssemblyValueType.ExternRef), new End()]],
        });
        Assert.AreEqual(ElementKind.PassiveExpressions, passiveExprs.Kind);
        Assert.IsTrue(passiveExprs.UsesExpressions);
        Assert.AreEqual(ElementType.ExternRef, passiveExprs.ElemType);
        Assert.AreEqual(1, passiveExprs.InitExprs.Count);

        // Declarative, initializer expressions, funcref.
        var declarativeExprs = RoundTrip(new Element
        {
            Kind = ElementKind.DeclarativeExpressions,
            ElemType = ElementType.FunctionReference,
            InitExprs = [[new RefFunc(0), new End()]],
        });
        Assert.AreEqual(ElementKind.DeclarativeExpressions, declarativeExprs.Kind);
        Assert.IsTrue(declarativeExprs.IsDeclarative);
        Assert.IsTrue(declarativeExprs.UsesExpressions);
        Assert.AreEqual(ElementType.FunctionReference, declarativeExprs.ElemType);
    }
}
