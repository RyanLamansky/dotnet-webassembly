using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the spec's stack-polymorphic typing in unreachable code, where popped operands and certain results
/// are an "unknown" type that matches any expected type.
/// </summary>
[TestClass]
public class PolymorphicStackTests
{
    /// <summary>
    /// After <c>unreachable</c>, an untyped <c>select</c> produces the polymorphic unknown value, which
    /// <c>ref.is_null</c> then accepts — so the function is valid and the module compiles.
    /// </summary>
    [TestMethod]
    public void UnreachableSelectThenRefIsNull_Compiles()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [] });
        module.Functions.Add(new Function { Type = 0 });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Unreachable(),
                new Select(),
                new RefIsNull(),
                new Drop(),
                new End(),
            ],
        });

        Assert.IsNotNull(module.ToInstance<object>());
    }
}
