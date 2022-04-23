using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Capsaicin.Util;
internal static class StackExtensions
{
    public static bool TryPop<T>(this Stack<T> stack, [MaybeNullWhen(false)]out T item)
    {
        if (stack.Count > 0)
        {
            item = stack.Pop();
            return true;
        }

        item = default;
        return false;
    }
}
