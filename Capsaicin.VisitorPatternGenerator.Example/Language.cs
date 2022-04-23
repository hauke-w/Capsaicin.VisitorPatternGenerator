using Capsaicin.VisitorPattern;
namespace Capsaicin.VisitorPatternGenerator.Example;

[VisitorPattern]
public abstract partial class Language
{
}

public partial class English : Language
{
}

public partial class German : Language
{
}
