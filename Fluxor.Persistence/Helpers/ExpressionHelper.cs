// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Reflection;

namespace Fluxor.Persistence.Helpers;

internal static class ExpressionHelper
{
    public static List<PropertyInfo> GetPropertyPath<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
    {
        var properties = new List<PropertyInfo>();
        Expression? expr = propertySelector.Body;
        while (expr is MemberExpression memberExpr)
        {
            if (memberExpr.Member is PropertyInfo propInfo)
            {
                properties.Insert(0, propInfo);
                expr = memberExpr.Expression;
            }
            else
            {
                throw new ArgumentException("Expression is not a property access.");
            }
        }

        if (properties.Count == 0)
        {
            throw new ArgumentException("Expression is not a property access.");
        }

        return properties;
    }

    public static string GetPropertyPathString<TState, TProperty>(Expression<Func<TState, TProperty>> expression)
    {
        IEnumerable<string> properties = GetPropertyPath(expression).Select(x => x.Name);
        return string.Join(".", properties);
    }
}
