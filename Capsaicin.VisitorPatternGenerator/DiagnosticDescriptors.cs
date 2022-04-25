using Capsaicin.VisitorPattern;
using Microsoft.CodeAnalysis;

namespace Capsaicin.VisitorPatternGenerator;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor DuplicateTypeSignature = new DiagnosticDescriptor("VP0001", "Duplicate visitor interface", $"A visitor interface with same signature was derived from the {nameof(VisitorPatternAttribute)}s", "Visitor Interface", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ParameterNamesDoNotMatchTypes = new DiagnosticDescriptor("VP0002", "Visitor parameter names do not match types", $"The number of visitor parameter names does not match the parameter types", "Visitor Interface", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor VisitorMethodFormatNotSpecified = new DiagnosticDescriptor("VP0003", "Visitor method format is not specified", $"{nameof(VisitorPatternAttribute.VisitorMethodFormat)} must be specified when {nameof(VisitorPatternAttribute.VisitorMethodRegex)} is set", "Visit Method", DiagnosticSeverity.Error, true);
    
    public static readonly DiagnosticDescriptor VisitorMethodRegexNotSpecified = new DiagnosticDescriptor("VP0004", "Visitor method regex is not specified", $"{nameof(VisitorPatternAttribute.VisitorMethodRegex)} is not specified although {nameof(VisitorPatternAttribute.VisitorMethodFormat)} is set. The default method formatting will be applied.", "Visit Method", DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor InvalidVisitorMethodRegex = new DiagnosticDescriptor("VP0005", "Visitor method regex or format is invalid", "The Visitor method regular expression or format is invalid: {0}", "Visit Method", DiagnosticSeverity.Error, true);
}
