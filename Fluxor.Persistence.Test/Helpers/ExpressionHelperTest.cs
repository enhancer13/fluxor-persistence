// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Reflection;
using Fluxor.Persistence.Helpers;
using Shouldly;

namespace Fluxor.Persistence.Test.Helpers;

public class ExpressionHelperTest
{
    [Fact]
    public void GivenSinglePropertyExpression_WhenGettingPropertyPath_ExpectSingleProperty()
    {
        // Arrange
        Expression<Func<SampleClass, string>> selector = x => x.Name!;

        // Act
        List<PropertyInfo> properties = ExpressionHelper.GetPropertyPath(selector);

        // Assert
        properties.Count.ShouldBe(1);
        properties[0].Name.ShouldBe("Name");
    }

    [Fact]
    public void GivenNestedPropertyExpression_WhenGettingPropertyPath_ExpectCorrectPropertyPath()
    {
        // Arrange
        Expression<Func<SampleClass, string?>> selector = x => x.Nested!.Description;

        // Act
        List<PropertyInfo> properties = ExpressionHelper.GetPropertyPath(selector);

        // Assert
        properties.Count.ShouldBe(2);
        properties[0].Name.ShouldBe("Nested");
        properties[1].Name.ShouldBe("Description");
    }

    [Fact]
    public void GivenDeeplyNestedPropertyExpression_WhenGettingPropertyPath_ExpectCorrectPropertyPath()
    {
        // Arrange
        Expression<Func<SampleClass, DateTime>> selector = x => x.Nested!.InnerNested!.CreatedDate;

        // Act
        List<PropertyInfo> properties = ExpressionHelper.GetPropertyPath(selector);

        // Assert
        properties.Count.ShouldBe(3);
        properties[0].Name.ShouldBe("Nested");
        properties[1].Name.ShouldBe("InnerNested");
        properties[2].Name.ShouldBe("CreatedDate");
    }

    [Fact]
    public void GivenExpressionNotPropertyAccess_WhenGettingPropertyPath_ExpectArgumentException()
    {
        // Arrange
        Expression<Func<SampleClass, string>> selector = x => x.Name!.ToLower();

        // Act & Assert
        ArgumentException exception = Should.Throw<ArgumentException>(() => ExpressionHelper.GetPropertyPath(selector));
        exception.Message.ShouldBe("Expression is not a property access.");
    }

    [Fact]
    public void GivenDeeplyNestedPropertyExpression_WhenGettingPropertyPathString_ExpectCorrectPropertyPath()
    {
        // Arrange
        Expression<Func<SampleClass, DateTime>> selector = x => x.Nested!.InnerNested!.CreatedDate;

        // Act
        string propertyPath = ExpressionHelper.GetPropertyPathString(selector);

        // Assert
        propertyPath.ShouldBe("Nested.InnerNested.CreatedDate");
    }

    private class SampleClass
    {
        public string? Name { get; init; }

        public NestedClass? Nested { get; init; }
    }

    private class NestedClass
    {
        public string? Description { get; init; }

        public InnerNestedClass? InnerNested { get; init; }
    }

    private class InnerNestedClass
    {
        public DateTime CreatedDate { get; init; }
    }
}
