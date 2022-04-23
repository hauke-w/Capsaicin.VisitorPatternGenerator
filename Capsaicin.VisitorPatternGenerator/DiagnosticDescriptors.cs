using Capsaicin.VisitorPattern;
using Microsoft.CodeAnalysis;

namespace Capsaicin.VisitorPatternGenerator;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor DuplicateTypeSignature = new DiagnosticDescriptor("VP0001", "Duplicate visitor interface", $"A visitor interface with same signature was derived from the {nameof(VisitorPatternAttribute)}s", "Visitor Interface", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ParameterNamesDoNotMatchTypes = new DiagnosticDescriptor("VP0002", "Visitor parameter names do not match types", $"The number of visitor parameter names does not match the parameter types", "Visitor Interface", DiagnosticSeverity.Error, true);
}
