using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Capsaicin.Util;
internal static class StringExtensions
{
    [return: NotNullIfNotNull("s")]
    public static string? ToFirstLower(this string? s)
    {
        return s switch
        {
            { Length: 1 } => s.ToLower(),
            { Length: > 1 } when !char.IsLower(s[0]) => char.ToLower(s[0]) + s.Substring(1),
            _ => s
        };
    }

    [return: NotNullIfNotNull("s")]
    public static string? ToFirstUpper(this string? s)
    {
        return s switch
        {
            { Length: 1 } => s.ToUpper(),
            { Length: > 1 } when !char.IsUpper(s[0]) => char.ToUpper(s[0]) + s.Substring(1),
            _ => s
        };
    }
}
