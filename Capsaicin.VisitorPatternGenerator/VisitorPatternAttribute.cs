using System;
using System.Diagnostics;

namespace Capsaicin.VisitorPattern;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
[Conditional("IncludeVisitorPatternAttribute")]
internal sealed class VisitorPatternAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance without parameters
    /// </summary>
    public VisitorPatternAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance without parameters and automatically generated names
    /// </summary>
    /// <param name="parameterTypes"></param>
    public VisitorPatternAttribute(Type?[] parameterTypes)
    {
        ParameterTypes = parameterTypes;
    }

    /// <summary>
    /// Initializes a new instance with named parameters
    /// </summary>
    /// <param name="parameterTypes"></param>
    /// <param name="parameterNames"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public VisitorPatternAttribute(Type?[] parameterTypes, string?[] parameterNames)
    {
        ParameterTypes = parameterTypes ?? throw new ArgumentNullException(nameof(parameterTypes));
        ParameterNames = parameterNames ?? throw new ArgumentNullException(nameof(parameterNames));
        if (parameterNames.Length != parameterTypes.Length)
        {
            throw new ArgumentException($"Length of {nameof(parameterTypes)} and {nameof(parameterNames)} is not equal.");
        }
    }

    /// <summary>
    /// Specifies whether the Visit method has no return value (void).
    /// </summary>
    /// <value>true: the Visit method has no return value. false: the Visit method has a return value (type must be specified by generic parameter).</value>
    public bool IsVisitMethodVoid { get; init; }

    /// <summary>
    /// Optional: specifies the types Visit method's parameters. For null values a Type parameter will be generated.
    /// </summary>
    public Type?[]? ParameterTypes { get; }

    /// <summary>
    /// Specifies the names of the Visit method's optional parameters. Corresponds to <see cref="ParameterTypes"/>. If not specified, default names will be generated.
    /// </summary>
    public string?[]? ParameterNames { get; }
}
