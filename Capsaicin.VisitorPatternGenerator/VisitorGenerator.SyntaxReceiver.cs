using System.Collections.Generic;
using System.Linq;
using Capsaicin.VisitorPattern;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Capsaicin.VisitorPatternGenerator;

partial class VisitorGenerator
{
    private class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<INamedTypeSymbol> Types { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is TypeDeclarationSyntax typeDeclarationSyntax
                && context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax) is INamedTypeSymbol typeSymbol
                && HasVisitorPatternAttribute(typeSymbol))
            {
                Types.Add(typeSymbol);
            }
        }

        private static bool HasVisitorPatternAttribute(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.GetAttributes().Any(it => it.AttributeClass?.Name is "VisitorPattern" or nameof(VisitorPatternAttribute));
        }
    }
}
