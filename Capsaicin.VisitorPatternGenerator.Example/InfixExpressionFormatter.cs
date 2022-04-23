using System.Globalization;
using System.Text;

namespace Capsaicin.VisitorPatternGenerator.Example;
class InfixExpressionFormatter : IExpressionFormatter, IExpressionVisitor<StringBuilder>
{
    public static InfixExpressionFormatter Instance = new InfixExpressionFormatter();

    public string Format(Expression expression, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;
        var builder = new StringBuilder();
        expression.Visit(this, builder, culture);
        return builder.ToString();
    }

    private void AppendBinaryExpression(BinaryExpression expression, StringBuilder builder, CultureInfo culture, string @operator)
    {
        AppendOperand(expression.A);
        builder.Append(@operator);
        AppendOperand(expression.B);


        void AppendOperand(Expression operand)
        {
            var opHaslowerPrecedence = expression.Precedence > operand.Precedence;
            if (opHaslowerPrecedence)
            {
                builder.Append('(');
            }
            operand.Visit(this, builder, culture);
            if (opHaslowerPrecedence)
            {
                builder.Append(')');
            }
        }
    }

    public string Type => "infix";

    void IExpressionVisitor<StringBuilder>.VisitAddExpression(AddExpression expression, StringBuilder builder, CultureInfo culture) => AppendBinaryExpression(expression, builder, culture, " + ");

    void IExpressionVisitor<StringBuilder>.VisitDivisionExpression(DivisionExpression expression, StringBuilder builder, CultureInfo culture) => AppendBinaryExpression(expression, builder, culture, " / ");
    void IExpressionVisitor<StringBuilder>.VisitMultiplyExpression(MultiplyExpression expression, StringBuilder builder, CultureInfo culture) => AppendBinaryExpression(expression, builder, culture, " * ");
    void IExpressionVisitor<StringBuilder>.VisitNegationExpression(NegationExpression expression, StringBuilder builder, CultureInfo culture)
    {
        builder.Append('-');
        expression.Operand.Visit(this, builder, culture);
    }
    void IExpressionVisitor<StringBuilder>.VisitParenthesisExpression(ParenthesisExpression expression, StringBuilder builder, CultureInfo culture)
    {
        builder.Append('(');
        expression.Inner.Visit(this, builder, culture);
        builder.Append(')');
    }

    void IExpressionVisitor<StringBuilder>.VisitSubtractExpression(SubtractExpression expression, StringBuilder builder, CultureInfo culture) => AppendBinaryExpression(expression, builder, culture, " - ");

    void IExpressionVisitor<StringBuilder>.VisitVariableExpression(VariableExpression expression, StringBuilder builder, CultureInfo culture) => builder.Append(expression.Variable);

    void IExpressionVisitor<StringBuilder>.VisitNumberExpression(NumberExpression expression, StringBuilder builder, CultureInfo culture) => builder.Append(expression.Number.ToString(culture));
    void IExpressionVisitor<StringBuilder>.VisitFuncExpression(FuncExpression expression, StringBuilder builder, CultureInfo culture)
    {
        builder.Append(expression.Func);
        builder.Append('(');
        if (expression.Parameters.Length > 0)
        {
            expression.Parameters[0].Visit(this, builder, culture);
            for (int i = 1; i < expression.Parameters.Length; i++)
            {
                builder.Append(", ");
                expression.Parameters[i].Visit(this, builder, culture);
            }
        }
        builder.Append(')');
    }
}
