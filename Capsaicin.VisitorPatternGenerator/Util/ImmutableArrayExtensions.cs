using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Capsaicin.Util;
internal static class ImmutableArrayEnumeratorExtensions
{
    public static IEnumerator<T> Enumerator<T>(this ImmutableArray<T> immutableArray)
    {
        for (int i = 0; i < immutableArray.Length; i++)
        {
            yield return immutableArray[i];
        }
    }
}
