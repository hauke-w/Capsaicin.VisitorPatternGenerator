using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Capsaicin.VisitorPattern;

namespace Capsaicin.VisitorPatternGenerator.Example;

// Visitor with one generic input parameter and return type as second generic parameter (IsVisitMethodVoid is false).
// There is currently no implementation of the corresponding interface in this example.
//[VisitorPattern(new Type?[] { null })]
// Visitor with one generic input parameter and return type as second generic parameter (IsVisitMethodVoid is false).
// There is currently no implementation of the corresponding interface in this example.
[VisitorPattern(1, VisitorInterfaceName="IFoo", VisitorMethodRegex= @"(\.)+Dto", VisitorMethodFormat = "Visit$1")]
// Visitor with 2 input parameter, first is generic, second is CultureInfo. No return type.
// The corresponding interface is implemented by InfixExpressionFormatter and PrefixExpressionFormatter
[VisitorPattern(new Type?[] { null, typeof(CultureInfo) }, new string?[] { null, "culture" }, IsVisitMethodVoid = true)]
abstract partial record Expression
{
    public virtual int Precedence => 0;

    public static implicit operator Expression(double number)
        => new NumberExpression(number);

    public static implicit operator Expression(string variable)
        => new VariableExpression(variable);
}

abstract partial record UnaryExpression(Expression Operand) : Expression
{
}

partial record NumberExpression(double Number) : Expression
{
}

partial record VariableExpression(string Variable) : Expression
{
}

partial record NegationExpression(Expression Operand) : UnaryExpression(Operand)
{
}

partial record ParenthesisExpression(Expression Inner) : Expression
{
}

partial record FuncExpression(string Func, params Expression[] Parameters) : Expression
{
}

abstract partial record BinaryExpression(Expression A, Expression B) : Expression
{
    public abstract override int Precedence { get; }
}

partial record MultiplyExpression(Expression A, Expression B) : BinaryExpression(A, B)
{
    public override int Precedence => -2;
}

partial record AddExpression(Expression A, Expression B) : BinaryExpression(A, B)
{
    public override int Precedence => -3;
}

partial record SubtractExpression(Expression A, Expression B) : BinaryExpression(A, B)
{
    public override int Precedence => -3;
}

partial record DivisionExpression(Expression A, Expression B) : BinaryExpression(A, B)
{
    public override int Precedence => -1;
}