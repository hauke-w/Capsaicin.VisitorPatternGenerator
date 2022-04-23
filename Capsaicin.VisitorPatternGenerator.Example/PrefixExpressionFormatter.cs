using System.Globalization;
using System.Text;

namespace Capsaicin.VisitorPatternGenerator.Example;

class PrefixExpressionFormatter : IExpressionFormatter, IExpressionVisitor<StringBuilder>
{
    public static PrefixExpressionFormatter Instance = new PrefixExpressionFormatter();

    public string Format(Expression expression, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;
        var builder = new StringBuilder();
        expression.Visit(this, builder, culture);
        return builder.ToString();
    }

    private void AppendBinaryExpression(BinaryExpression expression, StringBuilder builder, CultureInfo culture, string @operator)
    {
        builder.Append(@operator);
        builder.Append(' ');
        expression.A.Visit(this, builder, culture);
        builder.Append(' ');
        expression.B.Visit(this, builder, culture);
    }

    public string Type => "prefix";

    void IExpressionVisitor<StringBuilder>.VisitAddExpression(AddExpression expression, StringBuilder builder, CultureInfo culture) => AppendBinaryExpression(expression, builder, culture, "+");

    void IExpressionVisitor<StringBuilder>.VisitDivisionExpression(DivisionExpression expression, StringBuilder builder, CultureInfo culture) => AppendBinaryExpression(expression, builder, culture, "/");
    void IExpressionVisitor<StringBuilder>.VisitMultiplyExpression(MultiplyExpression expression, StringBuilder builder, CultureInfo culture) => AppendBinaryExpression(expression, builder, culture, "*");
    void IExpressionVisitor<StringBuilder>.VisitNegationExpression(NegationExpression expression, StringBuilder builder, CultureInfo culture)
    {
        builder.Append('-');
        expression.Operand.Visit(this, builder, culture);
    }
    void IExpressionVisitor<StringBuilder>.VisitParenthesisExpression(ParenthesisExpression expression, StringBuilder builder, CultureInfo culture) => expression.Inner.Visit(this, builder, culture);

    void IExpressionVisitor<StringBuilder>.VisitSubtractExpression(SubtractExpression expression, StringBuilder builder, CultureInfo culture) => AppendBinaryExpression(expression, builder, culture, "-");

    void IExpressionVisitor<StringBuilder>.VisitVariableExpression(VariableExpression expression, StringBuilder builder, CultureInfo culture) => builder.Append(expression.Variable);

    void IExpressionVisitor<StringBuilder>.VisitNumberExpression(NumberExpression expression, StringBuilder builder, CultureInfo culture) => builder.Append(expression.Number.ToString(culture));

    void IExpressionVisitor<StringBuilder>.VisitFuncExpression(FuncExpression expression, StringBuilder builder, CultureInfo culture)
    {
        builder.Append(expression.Func);
        foreach (var p in expression.Parameters)
        {
            builder.Append(' ');
            p.Visit(this, builder, culture);
        }
    }
}