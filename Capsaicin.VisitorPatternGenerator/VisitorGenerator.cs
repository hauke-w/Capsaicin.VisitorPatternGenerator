using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Capsaicin.VisitorPattern;
using Microsoft.CodeAnalysis;

namespace Capsaicin.VisitorPatternGenerator;

/// <summary>
/// C# source generator for Visitor Pattern members
/// </summary>
[Generator]
public partial class VisitorGenerator : ISourceGenerator
{
    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is SyntaxReceiver syntaxReceiver)
        {
            var contextMap = new Dictionary<string, List<ExecuteContext>>();
            var visitorPatternAttribute = context.Compilation.GetTypeByMetadataName(typeof(VisitorPatternAttribute).FullName);

            foreach (var typeSymbol in syntaxReceiver.Types)
            {
                foreach (var attribute in typeSymbol.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, visitorPatternAttribute))
                    {
                        var executeContext = new ExecuteContext(context, typeSymbol, attribute);
                        if (!contextMap.TryGetValue(executeContext.FullSignature, out var executeContexts))
                        {
                            executeContexts = new();
                            contextMap.Add(executeContext.FullSignature, executeContexts);
                        }
                        executeContexts.Add(executeContext);
                    }
                }
            }

            foreach (var contextList in contextMap.Values)
            {
                if (contextList.Count > 1)
                {
                    var location = contextList[0].TypeSymbol.Locations[0];
                    var additionalLocations = contextList.Skip(1).Select(it => it.TypeSymbol.Locations[0]).ToList();
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DuplicateTypeSignature, location, additionalLocations));
                }
                else
                {
                    contextList[0].Generate();
                }
            }
        }
    }

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(GenerateVisitorPatternAttribute);
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private void GenerateVisitorPatternAttribute(GeneratorPostInitializationContext context)
    {
        var sourceStream = GetType().Assembly.GetManifestResourceStream(nameof(Capsaicin) + "." + nameof(Capsaicin.VisitorPatternGenerator) + "." + nameof(VisitorPatternAttribute) + ".cs");
        var reader = new StreamReader(sourceStream);
        var sourceCode = reader.ReadToEnd();
        context.AddSource(nameof(VisitorPatternAttribute), sourceCode);
    }
}
