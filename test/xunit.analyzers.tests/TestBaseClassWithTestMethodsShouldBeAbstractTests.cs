using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestBaseClassWithTestMethodsShouldBeAbstractTests
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new TestBaseClassWithTestMethodsShouldBeAbstract();

            [Fact]
            public async void DoesNotFindErrorForPublicClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public abstract class TestClass { [Xunit.Fact] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            //[Theory]
            //[InlineData("Xunit.Fact")]
            //[InlineData("Xunit.Theory")]
            //public async void FindsErrorForPrivateClass(string attribute)
            //{
            //    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
            //        "class TestClass { [" + attribute + "] public void TestMethod() { } }");

            //    Assert.Collection(diagnostics,
            //        d =>
            //        {
            //            Assert.Equal("Test classes must be public", d.GetMessage());
            //            Assert.Equal("xUnit1000", d.Descriptor.Id);
            //        });
            //}
        }
    }
}
