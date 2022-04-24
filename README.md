# Visitor Pattern Generator
This is a C# Source Generator for generating Visitor Pattern members. It searches for *VisitorPattern* attribute annotations on classes in your project and generates a Visitor interface and *Accept* methods for the subordinate class hierarchy within the project.
See the [Visitor Pattern](https://en.wikipedia.org/wiki/Visitor_pattern) article on Wikipedia for details about the design pattern.

## Usage
### Enabling source generator in your project
The source generator must be added to the C# project that contains one or more class hiearachies that you want to be visitable. You can do this with either a nuget package reference or a project reference.
#### Option 1: add Nuget package
Add the *Capsaicin.VisitorPatternGenerator* nuget package to the project. The package reference in the *.csproj* file should be similar as follows:
```xml
<ItemGroup>
  <PackageReference Include="Capsaicin.VisitorPatternGenerator" Version="0.1.0" PrivateAssets="all" />
</ItemGroup>
```
#### Option 2: add Project reference
Alternatively you can download the source code and add a reference to the project. Add `OutputItemType="Analyzer"` to project reference in the project's *.csproj* file in order to enable the source generator. You may also want to add `ReferenceOutputAssembly="false"` because a runtime dependency is not required.
```xml
<ItemGroup>
  <ProjectReference Include="..\Capsaicin.VisitorPatternGenerator\Capsaicin.VisitorPatternGenerator.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```
### Generating and implementing visitor for class hierarchy
#### Root class
Add a *VisitorPattern* attribute annotation to the root class of your class hierarchy for which you want to enable the visitor pattern. Important: the class must be partial!
The source generator supports both abstract and non-abstract classes as well as record classes.
Add `using Capsaicin.VisitorPattern;` to the head of the *.cs* file in order to access the *VisitorPatternAttribute* without specifying the namespace.
```C#
using Capsaicin.VisitorPattern;

[VisitorPattern]
partial class Example
{
}
```
The source generator will generate a `IExampleVisitor<TResult>` interface for this class. Moreover it will add a `TResult Accept<TResult>(IExampleVisitor<TResult> visitor)` method to the Example class.
If the class is abstract the Accept method will be generated as abstract otherwise as virtual. Moreover, a Visit method will be available in the visitor interface only if the class is not abstract.
#### Sub classes
Sub classes must be partial as well. They can be abstract or non-abstract. For abstract classes in the hierarchy, no Visit method exists in the visitor interface.
```C#
partial class ExampleSubClass : Example
{
}
```

Further configuration is not needed on sub classes. If you add the a *VisitorPatternAttribute* annotation to a sub class, another visitor Interface and members will be generated independently.

#### Implement visitor
Add a class to your project and implement the generated interface. In the example the interface is implemented explicitly but this is of course not required.
```C#
class ExampleVisitor : IExampleVisitor<string>
{
    string IExampleVisitor<string>.VisitExample() => "Example";
    string IExampleVisitor<string>.VisitExampleSubClass() => "ExampleSubClass";
}
```
#### Using the visitor
Invoke the *Accept* method of a visitable class and pass a visitor instance to it:
```C#
var visitor = new ExampleVisitor();
var example = new Example();
var result1 = example.Accept(visitor); // result1 = "Example"

example = new ExampleSubClass();
var result2 = example2.Accept(visitor); // result2 = "ExampleSubClass"
```
### Visitor pattern options
Per default the visitor interface has a type parameter for the return type of the Accept and Visit~ methods.
If this does not meet your requirements you can parameterize the *VisitorPattern* attribute to modify the interface and methods.

While there is no limit in the pure number of *VisitorPattern* attributes you can add to a class, the actual attribute combinations you can specify is limited by the number of generic type parameters of the visitor interfaces that will be generated (see [Invalid attribute combinations](#Invalid attribute combinations) below).
#### Methods with no return value (void)
Set `IsVisitMethodVoid = true` in the *VisitorPattern* annotation if you do not need/want a return value for the generated methods.
```C#
using Capsaicin.VisitorPattern;

[VisitorPattern(IsVisitMethodVoid = true)]
partial class Example { }

partial class ExampleSubClass : Example { }

class ExampleVisitor1 : IExampleVisitor
{
    void IExampleVisitor.VisitExample(Example example) => Console.WriteLine("Example");
    void IExampleVisitor.VisitExampleSubClass(ExampleSubClass example) => Console.WriteLine("ExampleSubClass");
}
```

#### Addding method arguments
You can add arguments to the Accept and Visit~ methods that can be either generic or of a certain type. For this purpose two overloads of the *VisitorPatternAttribute* constructor exist with arguments:
- `VisitorPatternAttribute(Type?[] parameterTypes)`:
  - *parameterTypes* specifies the number and types of the method arguments. For *null* values in the array, a generic type parameter will be used. The name of the arguments will be generated (param1, param2, ...).
- `VisitorPatternAttribute(Type?[] parameterTypes, string?[] parameterNames)`:
  - *parameterTypes* specifies the number and types of the method arguments. For *null* values in the array, a generic type parameter will be used.
  - *parameterNames* specifies the names of the arguments. For *null* values in the array, the name will be generated. The length of the array must be same as the length of the *parameterTypes*.

##### Example: Arguments with type of type parameter
```C#
using System;
using System.Globalization;
using Capsaicin.VisitorPattern;

[VisitorPattern(new Type?[]{ null })] // corresponds to IExampleVisitor<T1, TResult>, see ExampleVisitor1
[VisitorPattern(new Type?[]{ null, null })] // corresponds to IExampleVisitor<T1, T2, TResult>, see ExampleVisitor2
partial class Example { }

partial class ExampleSubClass : Example { }

class ExampleVisitor1 : IExampleVisitor<int, string>
{
    string IExampleVisitor.VisitExample<int, string>(Example example, int param1) => "$Example: param1={param1}";
    string IExampleVisitor.VisitExampleSubClass<int, string>(ExampleSubClass example, int param1) => $"ExampleSubClass: param1={param1}";
}

class ExampleVisitor2 : IExampleVisitor<double, CultureInfo, string>
{
    string IExampleVisitor.VisitExample<double, CultureInfo, string>(Example example, double param1, CultureInfo param2) => "$Example: param1={param1.ToString(param2)}";
    string IExampleVisitor.VisitExampleSubClass<double, CultureInfo, string>(ExampleSubClass example, double param1, CultureInfo param2) => $"ExampleSubClass: param1={param1.ToString(param2)}";
}
```

Usage:
```C#
var visitor = new ExampleVisitor1();
Example example = new Example();
// No need to specify the type parameters explicitly. The compiler derives them from the parameters.
var result1 = example.Accept(visitor, 4711); // result1 = "Example: param1=4711"

example = new ExampleSubClass();
var result2 = example2.Accept(visitor, 4712); // result2 = "ExampleSubClass: param1=4712"
```

##### Example: Arguments of certain type
```C#
using System;
using System.Globalization;
using Capsaicin.VisitorPattern;

[VisitorPattern(new Type?[]{ typeof(CultureInfo) })] // corresponds to IExampleVisitor<TResult>, see ExampleVisitor3
[VisitorPattern(new Type?[]{ null, typeof(CultureInfo) })] // corresponds to IExampleVisitor<T1,  TResult>, see ExampleVisitor4
partial class Example { }

partial class ExampleSubClass : Example { }

class ExampleVisitor3 : IExampleVisitor<string>
{
    string IExampleVisitor.VisitExample<string>(Example example, CultureInfo param1) => "$Example: culture={param1.DisplayName}";
    string IExampleVisitor.VisitExampleSubClass<string>(ExampleSubClass example, CultureInfo param1) => $"ExampleSubClass: culture={param1.DisplayName}";
}

class ExampleVisitor4 : IExampleVisitor<double, string>
{
    string IExampleVisitor.VisitExample<double, string>(Example example, double param1, CultureInfo param2) => "$Example: param1={param1.ToString(param2)}");
    string IExampleVisitor.VisitExampleSubClass<double, string>(ExampleSubClass example, double param1, CultureInfo param2) => $"ExampleSubClass: param1={param1.ToString(param2)}";
}
```

##### Example: named Arguments
```C#
using System;
using System.Globalization;
using Capsaicin.VisitorPattern;

[VisitorPattern(new Type?[]{ typeof(CultureInfo) }, new[]{ "culture" })] // corresponds to IExampleVisitor<TResult>, see ExampleVisitor3
[VisitorPattern(new Type?[]{ null, typeof(CultureInfo) }, new[]{ null, "culture" })] // corresponds to IExampleVisitor<T1,  TResult>, see ExampleVisitor4
partial class Example { }

partial class ExampleSubClass : Example { }

class ExampleVisitor3 : IExampleVisitor<string>
{
    string IExampleVisitor.VisitExample<string>(Example example, CultureInfo culture) => "$Example: culture={culture.DisplayName}");
    string IExampleVisitor.VisitExampleSubClass<string>(ExampleSubClass example, CultureInfo culture) => $"ExampleSubClass: culture={culture.DisplayName}");
}

class ExampleVisitor4 : IExampleVisitor<double, string>
{
    string IExampleVisitor.VisitExample<double, string>(Example example, double param1, CultureInfo culture) => "$Example: param1={culture.ToString(culture)}");
    string IExampleVisitor.VisitExampleSubClass<double, string>(ExampleSubClass example, double param1, CultureInfo culture) => $"ExampleSubClass: param1={culture.ToString(culture)}";
}
```

### Invalid VisitorPattern attribute combinations
The valid combinations of *VisitorPattern* attribute annotations is determinded by the number of generic type parameters that will be generated. There can only be one visitor with 0, 1, 2 etc. type parameters, respectively.
Please mind that a type parameter is used for the return type unless you specify `IsVisitMethodVoid = true`.
Consequently, you cannot combine for example `[VisitorPattern]` and `[VisitorPattern(new Type?[]{ null }, IsVisitMethodVoid = true)]` because the visitor interface would have one parameter for both (`IExampleVisitor<TResult>` and `IExampleVisitor<T1>`). 

### Further examples
See the source code of the *Capsaicin.VisitorPatternGenerator.Example* project for examples.

#### Expression example
The Expressions example mixes the Visitor pattern with the Strategy pattern: there are two *IExpressionFormatter* implementations (*InfixExpressionFormatter* and *PrefixExpressionFormatter*) for printing an expression either in infix or prefix notation. Both formatters are implemented based on the visitor pattern (they implement the visitor `IExpressionVisitor<T1>` interface).

The example console application produces following output for this example:
```
*** Example 2: Expression formatting ***
infix: 2.5 * a * (b + -c + 3) / (3 * x)
prefix: * * 2.5 a / + b + -c 3 * 3 x

infix: (4 + 9) / (17 + 2)
prefix: / + 4 9 + 17 2

infix: (4 + 9) / sin(b + 7)
prefix: / + 4 9 sin + b 7

infix: 1 + 2 + 3 + 4
prefix: + 1 + 2 + 3 4

infix: 1 * 2 * 3 * 4
prefix: * 1 * 2 * 3 4
```

## VisitorPatternAttribute class details
The *Capsaicin.VisitorPattern.VisitorPatternAttribute* class is an internal class the source generator adds to your project. It is applicable to classes only.

Per default the *VisitorPatternAttribute* class class is not available at runtime only because it is marked as [Conditional](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.conditionalattribute) for the *IncludeVisitorPatternAttribute* compile time constant.
So if you want to use this attribute class at runtime via reflection, you must define the *IncludeVisitorPatternAttribute* constant in your project.