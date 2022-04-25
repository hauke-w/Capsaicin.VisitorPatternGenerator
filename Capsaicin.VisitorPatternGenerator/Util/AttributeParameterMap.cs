using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Capsaicin.VisitorPatternGenerator.Util;
internal static class AttributeParameterMap
{
    public static TValue? GetValueOrDefault<TValue>(this Dictionary<string, TypedConstant> parameterMap, string parameterName)
    {
        if (parameterMap.TryGetValue(parameterName, out var param))
        {
            return (TValue?)param.Value;
        }
        return default;
    }

    public static TValue?[]? GetValuesOrDefault<TValue>(this Dictionary<string, TypedConstant> parameterMap, string parameterName)
    {
        if (parameterMap.TryGetValue(parameterName, out var param)
            && !param.IsNull)
        {
            return param.Values.Select(it => (TValue?)it.Value).ToArray();
        }
        return null;
    }
}
