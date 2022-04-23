using System.Globalization;

namespace Capsaicin.VisitorPatternGenerator.Example;

interface IExpressionFormatter
{
    string Format(Expression expression, CultureInfo? culture = null);

    string Type { get; }
}
