// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Fluxor.Persistence.Strategies;
using Shouldly;

namespace Fluxor.Persistence.Test.Strategies;

public class PropertyAccessorTests
{
    [Fact]
    public void GivenPropertyWithPublicGetter_WhenGettingValue_ExpectValueReturned()
    {
        // Arrange
        var sample = new SampleClass("TestNameA", 10);
        var accessor = new PropertyAccessor<SampleClass, string>(x => x.Name);

        // Act
        string? value = accessor.Getter(sample);

        // Assert
        value.ShouldBe("TestNameA");
    }

    [Fact]
    public void GivenNestedPropertyWithPublicGetter_WhenGettingValue_ExpectValueReturned()
    {
        // Arrange
        var sample = new SampleClass("TestNameB", 20)
        {
            Nested = new NestedClass { InnerNestedClass = new InnerNestedClass { Description = "TestDescriptionB" } }
        };
        var accessor = new PropertyAccessor<SampleClass, string?>(x => x.Nested!.InnerNestedClass!.Description);

        // Act
        string? value = accessor.Getter(sample);

        // Assert
        value.ShouldBe("TestDescriptionB");
    }

    [Fact]
    public void GivenPropertyWithNestedNullIntermediate_WhenGettingValue_ExpectInvalidStateException()
    {
        // Arrange
        var sample = new SampleClass("TestNameC", 30) { Nested = null };
        var accessor = new PropertyAccessor<SampleClass, string?>(x => x.Nested!.InnerNestedClass!.Description);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => accessor.Getter(sample));
        exception.Message.ShouldBe("Cannot access property 'InnerNestedClass' because 'Nested' is null.");
    }

    [Fact]
    public void GivenPropertyWithPublicSetter_WhenSettingValue_ExpectValueUpdated()
    {
        // Arrange
        var sample = new SampleClass("TestNameD", 40);
        var accessor = new PropertyAccessor<SampleClass, string>(x => x.Name);

        // Act
        accessor.Setter(sample, "UpdatedName");

        // Assert
        sample.Name.ShouldBe("UpdatedName");
    }

    [Fact]
    public void GivenNestedPropertyWithPublicSetter_WhenSettingValue_ExpectValueUpdated()
    {
        // Arrange
        var sample = new SampleClass("TestNameE", 50) { Nested = new NestedClass { InnerNestedClass = new InnerNestedClass { Description = "InitialDescription" } } };
        var accessor = new PropertyAccessor<SampleClass, string?>(x => x.Nested!.InnerNestedClass!.Description);

        // Act
        accessor.Setter(sample, "UpdatedDesc");

        // Assert
        sample.Nested.InnerNestedClass.Description.ShouldBe("UpdatedDesc");
    }

    [Fact]
    public void GivenPropertyWithPrivateSetter_WhenSettingValue_ExpectValueUpdated()
    {
        // Arrange
        var sample = new SampleClass("TestNameG", 70);
        var accessor = new PropertyAccessor<SampleClass, int>(x => x.Age);

        // Act
        accessor.Setter(sample, 75);

        // Assert
        sample.Age.ShouldBe(75);
    }

    [Fact]
    public void GivenPropertyWithoutSetter_WhenCreatingAccessor_ExpectArgumentException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(
            () => new PropertyAccessor<NoSetterClass, string>(x => x.ReadOnlyProperty)
        );
        exception.Message.ShouldBe("Property 'ReadOnlyProperty' has no setter.");
    }

    [Fact]
    public void GivenInitOnlyProperty_WhenSettingValue_ExpectValueUpdated()
    {
        // Arrange
        var sample = new InitOnlyClass("InitialValue");
        var accessor = new PropertyAccessor<InitOnlyClass, string>(x => x.ReadOnlyInitProperty);

        // Act
        accessor.Setter(sample, "UpdatedValue");

        // Assert
        sample.ReadOnlyInitProperty.ShouldBe("UpdatedValue");
    }

    [Fact]
    public void GivenNestedPropertyWithNestedNullIntermediate_WhenSettingValue_ExpectInvalidStateException()
    {
        // Arrange
        var sample = new SampleClass("TestNameF", 60) { Nested = null };
        var accessor = new PropertyAccessor<SampleClass, string?>(x => x.Nested!.InnerNestedClass!.Description);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => accessor.Setter(sample, "NewDesc"));
        exception.Message.ShouldBe("Cannot access property 'InnerNestedClass' because 'Nested' is null.");
    }

    [Fact]
    public void GivenExpressionWithNotPropertyAccess_WhenCreatingAccessor_ExpectArgumentException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(
            () => new PropertyAccessor<SampleClass, string>(x => x.Name.ToUpper())
        );
        exception.Message.ShouldBe("Expression is not a property access.");
    }

    private class SampleClass
    {
        public string Name { get; set; }
        public int Age { get; private set; }
        public NestedClass? Nested { get; set; }

        public SampleClass(string name, int age)
        {
            Name = name;
            Age = age;
            Nested = new NestedClass();
        }
    }

    private class NestedClass
    {
        public InnerNestedClass? InnerNestedClass { get; set; }
    }

    private class InnerNestedClass
    {
        public string? Description { get; set; }
    }

    private class NoSetterClass
    {
        public string ReadOnlyProperty { get; }

        public NoSetterClass(string value)
        {
            ReadOnlyProperty = value;
        }
    }

    private class InitOnlyClass
    {
        public string ReadOnlyInitProperty { get; init; }

        public InitOnlyClass(string value)
        {
            ReadOnlyInitProperty = value;
        }
    }
}
