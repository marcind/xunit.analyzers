using System.Collections.Immutable;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestBaseClassWithTestMethodsShouldBeAbstract : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Constants.Descriptors.X1015_TestBaseClassShouldBeAbstract);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var factType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute);
                if (factType == null)
                    return;

                var concreteClassesWithTestMethodsCachce = new ConcurrentDictionary<INamedTypeSymbol, bool>();
                var classesWithConcreteSubclassCache = new ConcurrentDictionary<INamedTypeSymbol, bool>();

                compilationStartContext.RegisterSyntaxNodeAction(ctx =>
                {
                    var n = ctx.Node;
                }, SyntaxKind.ClassDeclaration);
                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var typeSymbol = (INamedTypeSymbol)symbolContext.Symbol;
                    if (typeSymbol.TypeKind != TypeKind.Class)
                        return;

                    var syntax = typeSymbol.DeclaringSyntaxReferences.First();

                    if (typeSymbol.IsAbstract)
                        return;

                    var hasTestMethods = typeSymbol.GetMembers()
                        .Where(m => m.Kind == SymbolKind.Method && ((IMethodSymbol)m).MethodKind == MethodKind.Ordinary)
                        .Any(m => m.GetAttributes().ContainsAttributeType(factType));

                    if (hasTestMethods)
                    {
                        concreteClassesWithTestMethodsCachce.TryAdd(typeSymbol, true);
                    }

                    var baseTypeSymbol = typeSymbol.BaseType;
                    while (baseTypeSymbol != symbolContext.Compilation.ObjectType)
                    {
                        classesWithConcreteSubclassCache.TryAdd(baseTypeSymbol, true);
                        baseTypeSymbol = baseTypeSymbol.BaseType;
                    }
                }, SymbolKind.NamedType);

                compilationStartContext.RegisterCompilationEndAction(compilationEndContext =>
                {
                    foreach (var kvp in classesWithConcreteSubclassCache)
                    {
                        var cls = kvp.Key;
                        if (!cls.IsAbstract && concreteClassesWithTestMethodsCachce.ContainsKey(cls))
                        {
                            compilationEndContext.ReportDiagnostic(Diagnostic.Create(
                                Constants.Descriptors.X1015_TestBaseClassShouldBeAbstract,
                                cls.Locations[0],
                                cls.Name));
                        }
                    }
                });
            });
        }
    }
}
