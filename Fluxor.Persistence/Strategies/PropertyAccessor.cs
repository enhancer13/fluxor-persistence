// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Reflection;
using Fluxor.Persistence.Helpers;

namespace Fluxor.Persistence.Strategies;

internal sealed class PropertyAccessor<T, TProperty>
{
    public PropertyAccessor(Expression<Func<T, TProperty>> propertySelector)
    {
        List<PropertyInfo> properties = ExpressionHelper.GetPropertyPath(propertySelector);
        Getter = CreateGetter(properties);
        Setter = CreateSetter(properties);
    }

    public Func<T, TProperty?> Getter { get; }

    public Action<T, TProperty?> Setter { get; }

    private static Func<T, TProperty?> CreateGetter(List<PropertyInfo> properties)
    {
        ParameterExpression instanceParam = Expression.Parameter(typeof(T), "instance");
        Expression chain = BuildNullSafeChain(properties, instanceParam);
        return Expression.Lambda<Func<T, TProperty>>(chain, instanceParam).Compile();
    }

    private static Action<T, TProperty?> CreateSetter(List<PropertyInfo> properties)
    {
        PropertyInfo finalProperty = properties[^1];
        MethodInfo setMethod = finalProperty.GetSetMethod(true) ?? throw new ArgumentException($"Property '{finalProperty.Name}' has no setter.");

        ParameterExpression instanceParam = Expression.Parameter(typeof(T), "instance");
        ParameterExpression valueParam = Expression.Parameter(typeof(TProperty), "value");

        Expression chain = BuildNullSafeChain(properties.Take(properties.Count - 1).ToList(), instanceParam);
        MethodCallExpression callSetter = Expression.Call(
            chain,
            setMethod,
            Expression.Convert(valueParam, finalProperty.PropertyType)
        );

        return Expression.Lambda<Action<T, TProperty?>>(callSetter, instanceParam, valueParam).Compile();
    }

    private static Expression BuildNullSafeChain(List<PropertyInfo> properties, Expression instance)
    {
        Expression current = instance;
        for (int i = 0; i < properties.Count; i++)
        {
            PropertyInfo property = properties[i];
            BinaryExpression nullCheck = Expression.Equal(current, Expression.Constant(null, current.Type));
            string message = i == 0
                ? $"Cannot access property '{property.Name}' because the instance is null."
                : $"Cannot access property '{property.Name}' because '{properties[i - 1].Name}' is null.";

            UnaryExpression throwEx = Expression.Throw(
                Expression.New(
                    typeof(InvalidOperationException).GetConstructor([typeof(string)])!,
                    Expression.Constant(message)
                ),
                property.PropertyType
            );

            current = Expression.Condition(
                nullCheck,
                throwEx,
                Expression.Property(current, property)
            );
        }
        return current;
    }
}
