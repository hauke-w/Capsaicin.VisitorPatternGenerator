using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Capsaicin.Util;
using Microsoft.CodeAnalysis;

namespace Capsaicin.VisitorPatternGenerator.Util;
internal static class NamespaceSymbolExtensions
{
    public static IEnumerable<INamedTypeSymbol> FlattenTypeMembers(this INamespaceSymbol root)
    {
        var nsStack = new Stack<IEnumerator<INamespaceSymbol>>();
        var currentNamespaceEnumerator = new[] { root }.AsEnumerable().GetEnumerator();
        do
        {
            while (currentNamespaceEnumerator.MoveNext())
            {
                var typeStack = new Stack<IEnumerator<INamedTypeSymbol>>();
                IEnumerator<INamedTypeSymbol>? typeEnumerator = currentNamespaceEnumerator.Current.GetTypeMembers().Enumerator();
                do
                {
                    while (typeEnumerator.MoveNext())
                    {
                        yield return typeEnumerator.Current;
                        var children = typeEnumerator.Current.GetTypeMembers();

                        if (!children.IsEmpty)
                        {
                            typeStack.Push(typeEnumerator);
                            typeEnumerator = children.Enumerator();
                        }
                    }
                } while (typeStack.TryPop(out typeEnumerator));

                var childNamespaces = currentNamespaceEnumerator.Current.GetNamespaceMembers().GetEnumerator();
                nsStack.Push(currentNamespaceEnumerator);
                currentNamespaceEnumerator = childNamespaces;
            }
        } while (nsStack.TryPop(out currentNamespaceEnumerator));
    }
}
