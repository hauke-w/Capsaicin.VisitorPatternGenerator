using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Capsaicin.VisitorPattern;

/// <summary>
/// The VisitorPatternGenerator will generate a visitor interface and and an Accept method for classes annotated with this attribute. The attribute must be applied to the root class of the hierarchy.
/// </summary>
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
    /// Initializes a new instance with the specified number of parameters (<paramref name="numberOfParameters"/>).
    /// </summary>
    /// <param name="numberOfParameters">Specifies the number of the method arguments.</param>
    public VisitorPatternAttribute(int numberOfParameters)
        : this(new Type?[numberOfParameters])
    {
    }

    /// <summary>
    /// Initializes a new instance with parameters and automatically generated names
    /// </summary>
    /// <param name="parameterTypes">Specifies the number and types of the method arguments. For null values in the array, a generic type parameter will be used.</param>
    public VisitorPatternAttribute(Type?[] parameterTypes)
    {
        ParameterTypes = parameterTypes;
    }

    /// <summary>
    /// Initializes a new instance with named parameters
    /// </summary>
    /// <param name="parameterTypes">Specifies the number and types of the method arguments. For null values in the array, a generic type parameter will be used.</param>
    /// <param name="parameterNames">
    /// Specifies the names of the arguments. For null values in the array, the name will be generated.
    /// The length of the array must be same as the length of the <paramref name="parameterTypes"/>.
    /// </param>
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
    /// Specifies whether the Accept / Visit method have no return value (void).
    /// </summary>
    /// <value>true: the Accept / Visit methods have no return value. false: the methods have a return value (type must be specified by generic parameter).</value>
    public bool IsVisitMethodVoid { get; init; }

    /// <summary>
    /// Optional: specifies the types Accept method's parameters. For null values a Type parameter will be generated.
    /// </summary>
    public Type?[]? ParameterTypes { get; }

    /// <summary>
    /// Specifies the names of the Accept method's optional parameters. Corresponds to <see cref="ParameterTypes"/>. If not specified, default names will be generated.
    /// </summary>
    public string?[]? ParameterNames { get; }

    /// <summary>
    /// The name of the Visitor interface to generate or null. If null, the name will be generated from the annotated class.
    /// </summary>
    public string? VisitorInterfaceName { get; init; }

    /// <summary>
    /// Regular expression for formatting the name of the Visitor method names. When this property is set, the <see cref="VisitorMethodFormat"/> property must be set as well.
    /// </summary>
    /// <remarks>
    /// The name will be constructed using <see cref="Regex.Replace(string, string, string)"/> with <see cref="VisitorMethodRegex"/> as pattern and <see cref="VisitorMethodFormat"/> as replacement:
    /// <c>MethodName=Regex.Replace(type.Name, VisitorMethodRegex, VisitorMethodFormat);</c>
    /// 
    /// <para>
    /// Example: remove suffix from class name<br/>
    /// <c>VisitorMethodFormat = "Visit$1"</c><br/>
    /// <c>VisitorMethodRegex = "(.+)Dto" </c>.<br/>
    /// For the class "MyDto" the Visit-method name will be "VisitMy".
    /// </para>
    /// </remarks>
    public string? VisitorMethodRegex { get; init; }

    /// <summary>
    /// Replacement expression for formatting the name of the Visitor method names. When this property is set, the <see cref="VisitorMethodRegex"/> property should be set as well. This property is ignored if <see cref="VisitorMethodRegex"/> is not set.
    /// </summary>
    /// <remarks>
    /// The name will be constructed using <see cref="Regex.Replace(string, string, string)"/> with <see cref="VisitorMethodRegex"/> as pattern and <see cref="VisitorMethodFormat"/> as replacement:
    /// <c>MethodName=Regex.Replace(type.Name, VisitorMethodRegex, VisitorMethodFormat);</c>
    /// </remarks>
    public string? VisitorMethodFormat { get; init; }
}
