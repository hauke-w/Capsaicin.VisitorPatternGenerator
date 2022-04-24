using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capsaicin.VisitorPatternGenerator.Example;

// Example 1: print message created by visitor
Console.WriteLine("*** Example 1: print message created by visitor ***");
var visitor = new SayHelloVisitor();
Language language = new English();
Console.WriteLine(language.Accept(visitor));

language = new German();
Console.WriteLine(language.Accept(visitor));

// Example 2: Expression formatting with formatters that implement the visitor pattern (see InfixExpressionFormatter and PrefixExpressionFormatter)
Console.WriteLine();
Console.WriteLine("*** Example 2: Expression formatting ***");
var expression1 = new MultiplyExpression(
    new MultiplyExpression(2.5, "a"),
    new DivisionExpression(
        new AddExpression(
            "b",
            new AddExpression(
                new NegationExpression("c"),
                3d)),
        new MultiplyExpression(3, "x")
        )
    );
PrintExpression(expression1);

var expression2 = new DivisionExpression(
    new AddExpression(4, 9),
    new AddExpression(17, 2));
PrintExpression(expression2);

var expression3 = new DivisionExpression(
    new AddExpression(4, 9),
    new FuncExpression("sin",
        new AddExpression("b", 7)));
PrintExpression(expression3);

var expression4 = new AddExpression(1,
    new AddExpression(2,
        new AddExpression(3, 4)));
PrintExpression(expression4);

var expression5 = new MultiplyExpression(1,
    new MultiplyExpression(2,
        new MultiplyExpression(3, 4)));
PrintExpression(expression5);

void PrintExpression(Expression expression)
{
    // the formatters are implemented using the Visitor pattern
    PrintExpression2(expression, InfixExpressionFormatter.Instance);
    PrintExpression2(expression, PrefixExpressionFormatter.Instance);
    Console.WriteLine();
}

void PrintExpression2(Expression expression, IExpressionFormatter formatter)
{
    var expessionString = formatter.Format(expression);
    Console.WriteLine($"{formatter.Type}: {expessionString}");
}

class SayHelloVisitor : ILanguageVisitor<string>
{
    public string VisitEnglish(English language) => "Hello World!";
    public string VisitGerman(German language) => "Hallo Welt!";
}