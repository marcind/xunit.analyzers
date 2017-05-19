using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.FixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class MakeAbstract : CodeFixProvider
    {
        const string title = "Make Abstract";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X1015_TestBaseClassShouldBeAbstract.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => MakePublicAsync(context.Document, classDeclaration, ct),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        async Task<Document> MakePublicAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            ISymbol symbol = (await document.GetSemanticModelAsync(cancellationToken)).GetDeclaredSymbol(classDeclaration, cancellationToken);
            var mods = DeclarationModifiers.From(symbol);
            editor.SetModifiers(classDeclaration, mods.WithIsAbstract(true));
            return editor.GetChangedDocument();
        }
    }
}