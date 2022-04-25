using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Capsaicin.VisitorPatternGenerator.Util;
internal static class TypedConstantExtensions
{
    public static ITypeSymbol? GetTypeValue(TypedConstant constant) => constant.IsNull ? null : (ITypeSymbol)constant.Value!;
}
