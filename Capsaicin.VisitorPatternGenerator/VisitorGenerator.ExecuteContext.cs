using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Capsaicin.Util;
using Capsaicin.VisitorPattern;
using Capsaicin.VisitorPatternGenerator.Util;
using Microsoft.CodeAnalysis;

namespace Capsaicin.VisitorPatternGenerator;

partial class VisitorGenerator
{
    private class ExecuteContext
    {
        public ExecuteContext(GeneratorExecutionContext generatorExecutionContext, INamedTypeSymbol typeSymbol, AttributeData attribute)
        {
            GeneratorExecutionContext = generatorExecutionContext;
            TypeSymbol = typeSymbol;
            Attribute = attribute;
            (Parameters, var visitorInterfaceName) = EvaluateAttribute();
            (InterfaceSignature, FullSignature, InterfaceHintName) = GenerateInterfaceName(visitorInterfaceName);

            var arguments = new List<string>(Parameters.Parameters.Count + 1) { "this" };
            arguments.AddRange(Parameters.Parameters.Select(it => it.Name.ToFirstLower()));
            VisitorArgumentValues = string.Join(", ", arguments);

            VisitMethodParameters = string.Concat(Parameters.Parameters.Select(it => string.Concat(", ", it.Type, " ", it.Name)));
            AcceptMethodSignature = $"{Parameters.ReturnType} Accept{Parameters.TypeParameters}({InterfaceSignature} visitor{VisitMethodParameters})";

            ObjectParameterName = typeSymbol.Name.ToFirstLower();
        }

        private readonly string VisitorArgumentValues;
        private readonly string VisitMethodParameters;
        private readonly string AcceptMethodSignature;

        private readonly string InterfaceHintName;
        private readonly string InterfaceSignature;

        private readonly string ObjectParameterName;

        private readonly GeneratorExecutionContext GeneratorExecutionContext;
        public INamedTypeSymbol TypeSymbol { get; }
        public AttributeData Attribute { get; }

        /// <summary>
        /// Used to detect duplicates
        /// </summary>
        public string FullSignature { get; }

        private readonly ParametersInfo Parameters;

        private record ParametersInfo(List<(string Type, string Name)> Parameters, string? TypeParameters, int TypeParameterCount, bool HasReturn, string ReturnType);

        private (string Signature, string FullSignature, string HintName) GenerateInterfaceName(string name)
        {
            var fullSignatureBuilder = new StringBuilder();
            var signatureBuilder = new StringBuilder();
            AppendContainingTypeOrNamespace(TypeSymbol);

            signatureBuilder.Append(name);
            var hintName = signatureBuilder.ToString();
            if (Parameters.TypeParameterCount > 0)
            {
                hintName = string.Concat(hintName, "`", Parameters.TypeParameterCount);
            }

            signatureBuilder.Append(Parameters.TypeParameters);

            var signature = signatureBuilder.ToString();
            fullSignatureBuilder.Append(signature);

            return (signature, fullSignatureBuilder.ToString(), hintName);

            void AppendContainingTypeOrNamespace(ISymbol symbol)
            {
                var containingType = symbol.ContainingType;
                if (containingType is not null)
                {
                    fullSignatureBuilder.Append(containingType.ToDisplayString());
                }
                else if (symbol.ContainingNamespace is { } ns)
                {
                    fullSignatureBuilder.Append(ns.ToDisplayString());
                }
                fullSignatureBuilder.Append(".");
            }
        }

        private bool IsInt(ITypeSymbol? type)
        {
            var intType = GeneratorExecutionContext.Compilation.GetSpecialType(SpecialType.System_Int32);
            return type is not null
                && SymbolEqualityComparer.Default.Equals(type, intType);
        }

        private (ParametersInfo Parameters, string VisitorInterfaceName) EvaluateAttribute()
        {
            var parameterMap = new Dictionary<string, TypedConstant>();
            IReadOnlyList<ITypeSymbol?> parameterTypes = Array.Empty<ITypeSymbol?>();

            if (Attribute.ConstructorArguments is { IsEmpty: false } constructorArguments)
            {
                if (constructorArguments.Length == 1 && IsInt(constructorArguments[0].Type))
                {
                    int numberOfParameters = (int)constructorArguments[0].Value!;
                    if (numberOfParameters > 0)
                    {
                        parameterTypes = new ITypeSymbol?[numberOfParameters];
                    }
                }
                else
                {
                    parameterMap[nameof(VisitorPatternAttribute.ParameterTypes)] = constructorArguments[0];
                    if (constructorArguments.Length > 1)
                    {
                        parameterMap[nameof(VisitorPatternAttribute.ParameterNames)] = constructorArguments[1];
                    }
                }
            }
            foreach (var item in Attribute.NamedArguments)
            {
                parameterMap[item.Key] = item.Value;
            }

            var visitorInterfaceName = EvaluateVisitorInterfaceName();
            return (EvaluateParameters(), visitorInterfaceName);

            string EvaluateVisitorInterfaceName()
            {
                string? name = null;
                if (parameterMap.TryGetValue(nameof(VisitorPatternAttribute.VisitorInterfaceName), out var nameParameter)
                    && !nameParameter.IsNull)
                {
                    name = (string)nameParameter.Value!;
                }

                return name is null or { Length: 0 }
                    ? $"I{TypeSymbol.Name}Visitor"
                    : name;
            }

            ParametersInfo EvaluateParameters()
            {
                bool isVoid = IsVoid();
                LoadParameterTypes();
                var parameterNames = LoadParameterNames();

                var typeParameterNames = new List<string>();
                var parameters = new List<(string Type, string Name)>();

                for (int i = 0; i < parameterNames.Length; i++)
                {
                    string? paramName = parameterNames[i];

                    var parameterType = parameterTypes[i];
                    string paramTypeName;
                    int parameterNumber = i + 1;
                    if (parameterType is null)
                    {
                        paramTypeName = !string.IsNullOrEmpty(paramName)
                            ? "T" + paramName.ToFirstUpper()
                            : "T" + parameterNumber;
                        typeParameterNames.Add(paramTypeName);
                    }
                    else
                    {
                        paramTypeName = parameterType.ToDisplayString();
                    }
                    parameters.Add((paramTypeName, paramName ?? "param" + parameterNumber));
                }

                string returnType;
                if (isVoid)
                {
                    returnType = "void";
                }
                else
                {
                    returnType = "TResult";
                    typeParameterNames.Add(returnType);
                }

                string? typeParameters = typeParameterNames.Count == 0
                    ? null
                    : $"<{string.Join(", ", typeParameterNames)}>";

                return new(parameters, typeParameters, typeParameterNames.Count, !isVoid, returnType);
            }

            bool IsVoid()
            {
                var isVoidArg = Attribute.NamedArguments.LastOrDefault(it => it.Key == nameof(VisitorPatternAttribute.IsVisitMethodVoid));
                object? val = isVoidArg.Value.Value;
                return parameterMap.TryGetValue(nameof(VisitorPatternAttribute.IsVisitMethodVoid), out var isVoidParam) && (bool)isVoidParam.Value!;
            }

            void LoadParameterTypes()
            {
                if (parameterMap.TryGetValue(nameof(VisitorPatternAttribute.ParameterTypes), out var parameterTypesParam))
                {
                    parameterTypes = parameterTypesParam.IsNull
                        ? Array.Empty<ITypeSymbol?>()
                        : parameterTypesParam.Values.Select(TypedConstantExtensions.GetTypeValue).ToArray();
                }
            }

            string?[] LoadParameterNames()
            {
                if (parameterMap.TryGetValue(nameof(VisitorPatternAttribute.ParameterNames), out var parameterNamesParam))
                {
                    if (parameterNamesParam.Values.Length != parameterTypes.Count)
                    {
                        GeneratorExecutionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ParameterNamesDoNotMatchTypes, TypeSymbol.Locations[0]));
                    }
                    return parameterNamesParam.Values.Select(it => (string?)it.Value).ToArray();
                }
                else
                {
                    return new string?[parameterTypes.Count];
                }
            }
        }

        internal void Generate()
        {
            var types = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            var negative = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var allTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            allTypes.Add(TypeSymbol);

            foreach (var type in TypeSymbol.ContainingAssembly.GlobalNamespace.FlattenTypeMembers())
            {
                if (!type.IsAbstract && type.TypeKind == TypeKind.Class
                    && TypeSymbolIsAssignableFrom(type))
                {
                    types.Add(type);
                }
            }

            GenerateVisitorInterface(types);

            types.Remove(TypeSymbol);
            GenerateAcceptMethod(TypeSymbol, true);
            foreach (var type in types)
            {
                GenerateAcceptMethod(type);
            }

            bool TypeSymbolIsAssignableFrom(INamedTypeSymbol t)
            {
                if (allTypes.Contains(t))
                {
                    return true;
                }
                else if (negative.Contains(t))
                {
                    return false;
                }
                else if (t.BaseType is { } baseType && TypeSymbolIsAssignableFrom(baseType))
                {
                    allTypes.Add(t);
                    return true;
                }
                else
                {
                    negative.Add(t);
                    return false;
                }
            }
        }

        private void GenerateVisitorInterface(HashSet<INamedTypeSymbol> nonAbstractTypes)
        {
            var builder = new StringBuilder();
            builder.AppendLine(@"using System;
using System.Collections.Generic;
using System.Linq;");
            if (TypeSymbol.ContainingNamespace is { IsGlobalNamespace: false } ns)
            {
                builder.AppendLine($"namespace {ns.ToDisplayString()};");
            }

            var accessModifier = Microsoft.CodeAnalysis.CSharp.SyntaxFacts.GetText(TypeSymbol.DeclaredAccessibility);
            if (!string.IsNullOrEmpty(accessModifier))
            {
                accessModifier = accessModifier + " ";
            }
            builder.AppendLine(@$"{accessModifier}interface {InterfaceSignature}");
            builder.AppendLine("{");
            var returnType = Parameters.ReturnType;
            foreach (var type in nonAbstractTypes)
            {
                builder.AppendLine($"   {returnType} Visit{type.Name}({type.ToDisplayString()} {ObjectParameterName}{VisitMethodParameters});");
            }
            builder.AppendLine("}");
            string source = builder.ToString();
            GeneratorExecutionContext.AddSource(InterfaceHintName, source);
        }

        private void GenerateAcceptMethod(INamedTypeSymbol type, bool isBase = false)
        {
            var builder = new StringBuilder();
            builder.AppendLine(@"using System;
using System.Collections.Generic;
using System.Linq;");
            if (type.ContainingNamespace is { IsGlobalNamespace: false } ns)
            {
                builder.AppendLine($"namespace {ns.ToDisplayString()};");

            }
            if (isBase && type.IsAbstract)
            {
                builder.Append("abstract ");
            }
            var kind = type.IsRecord ? "record" : "class";
            builder.Append($"partial {kind} {type.Name}");
            if (type.IsGenericType)
            {
                var parameters = type.TypeParameters;
                builder.Append('<');
                builder.Append(parameters[0].Name);
                for (int i = 1; i < parameters.Length; i++)
                {
                    builder.Append(", ");
                    builder.Append(parameters[1].Name);
                }
                builder.Append('>');
            }
            builder.AppendLine();

            string modifier = isBase switch
            {
                true when type.IsAbstract => "abstract ",
                true => "virtual ",
                _ => "override "
            };
            builder.AppendLine("{");
            builder.Append("    public ");
            builder.Append(modifier);
            builder.Append(AcceptMethodSignature);

            if (isBase && type.IsAbstract)
            {
                builder.AppendLine(";");
            }
            else
            {
                builder.AppendLine($" => visitor.Visit{type.Name}({VisitorArgumentValues});");
            }
            builder.AppendLine("}");
            string source = builder.ToString();
            string hintName = $"{type.Name}.Accept`{Parameters.TypeParameterCount}";
            GeneratorExecutionContext.AddSource(hintName, source);
        }
    }
}
