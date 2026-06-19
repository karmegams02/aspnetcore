// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.Forms;

public class HandlerFormTest
{
    private TestRenderer _testRenderer = new();

    public HandlerFormTest()
    {
        var services = new ServiceCollection();
        services.AddAntiforgery();
        services.AddLogging();
        services.AddSingleton<AntiforgeryStateProvider, DefaultAntiforgeryStateProvider>();
        _testRenderer = new(services.BuildServiceProvider());
    }

    #region Basic Rendering Tests

    [Fact]
    public async Task RendersFormElement()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent();

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
        Assert.Equal("form", formElement.ElementName);
    }

    [Fact]
    public async Task RendersFormWithMethodPost()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent();

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var methodAttribute = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "method");
        Assert.NotNull(methodAttribute.AttributeName);
        Assert.Equal("post", methodAttribute.AttributeValue);
    }

    [Fact]
    public async Task RendersChildContent()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder =>
            {
                builder.AddContent(0, "Form Content");
            }
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var textFrame = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Text && f.TextContent == "Form Content");
        Assert.NotNull(textFrame.TextContent);
        Assert.Equal("Form Content", textFrame.TextContent);
    }

    [Fact]
    public async Task RendersMultipleChildElements()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder =>
            {
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "type", "text");
                builder.CloseElement();
                builder.OpenElement(2, "button");
                builder.AddContent(3, "Submit");
                builder.CloseElement();
            }
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var inputElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "input");
        var buttonElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "button");

        Assert.NotNull(inputElement.ElementName);
        Assert.NotNull(buttonElement.ElementName);
        Assert.Equal("input", inputElement.ElementName);
        Assert.Equal("button", buttonElement.ElementName);
    }

    #endregion

    #region FormName Parameter Tests

    [Fact]
    public async Task AddsFormNameAttribute_WhenFormNameIsProvided()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = "myform"
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task DoesNotAddFormNameAttribute_WhenFormNameIsNull()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = null
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
        Assert.Equal("form", formElement.ElementName);
    }

    [Fact]
    public async Task DoesNotAddFormNameAttribute_WhenFormNameIsEmpty()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = string.Empty
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
        Assert.Equal("form", formElement.ElementName);
    }

    [Fact]
    public async Task DoesNotAddFormNameAttribute_WhenFormNameIsWhitespace()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = "   "
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    #endregion

    #region OnSubmit Callback Tests

    [Fact]
    public async Task AddsOnSubmitHandler_WhenOnSubmitHasDelegate()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            OnSubmit = EventCallback.Factory.Create(this, async () =>
            {
                await Task.CompletedTask;
            })
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var onsubmitAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == "onsubmit");

        Assert.NotNull(onsubmitAttribute.AttributeName);
    }

    [Fact]
    public async Task DoesNotAddOnSubmitHandler_WhenOnSubmitIsEmpty()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            OnSubmit = EventCallback.Empty
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    #endregion

    #region AdditionalAttributes Tests

    [Fact]
    public async Task AppliesAdditionalAttributes()
    {
        // Arrange
        var additionalAttributes = new Dictionary<string, object>
        {
            { "class", "custom-form" },
            { "id", "my-form" }
        };
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = additionalAttributes
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var classAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == "class");
        var idAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == "id");

        Assert.NotNull(classAttribute.AttributeName);
        Assert.Equal("custom-form", classAttribute.AttributeValue);
        Assert.NotNull(idAttribute.AttributeName);
        Assert.Equal("my-form", idAttribute.AttributeValue);
    }

    [Fact]
    public async Task AppliesDataAttributes()
    {
        // Arrange
        var additionalAttributes = new Dictionary<string, object>
        {
            { "data-testid", "form-test" },
            { "data-form-type", "registration" }
        };
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = additionalAttributes
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var dataTestIdAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == "data-testid");
        var dataFormTypeAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == "data-form-type");

        Assert.NotNull(dataTestIdAttribute.AttributeName);
        Assert.Equal("form-test", dataTestIdAttribute.AttributeValue);
        Assert.NotNull(dataFormTypeAttribute.AttributeName);
        Assert.Equal("registration", dataFormTypeAttribute.AttributeValue);
    }

    [Fact]
    public async Task AppliesAriaAttributes()
    {
        // Arrange
        var additionalAttributes = new Dictionary<string, object>
        {
            { "aria-label", "Contact Form" },
            { "aria-describedby", "form-help" }
        };
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = additionalAttributes
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var ariaLabelAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == "aria-label");
        var ariaDescribedByAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == "aria-describedby");

        Assert.NotNull(ariaLabelAttribute.AttributeName);
        Assert.Equal("Contact Form", ariaLabelAttribute.AttributeValue);
        Assert.NotNull(ariaDescribedByAttribute.AttributeName);
        Assert.Equal("form-help", ariaDescribedByAttribute.AttributeValue);
    }

    [Fact]
    public async Task HandlesNullAdditionalAttributes()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = null
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task HandlesEmptyAdditionalAttributes()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = new Dictionary<string, object>()
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    #endregion

    #region Antiforgery Token Tests

    [Fact]
    public async Task RendersAntiforgeryTokenComponent()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent();

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var antiforgeryComponent = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Component &&
            f.ComponentType == typeof(AntiforgeryToken));

        Assert.NotNull(antiforgeryComponent.ComponentType);
        Assert.Equal(typeof(AntiforgeryToken), antiforgeryComponent.ComponentType);
    }

    #endregion

    #region Combined Parameters Tests

    [Fact]
    public async Task RendersWithFormNameAndAdditionalAttributes()
    {
        // Arrange
        var additionalAttributes = new Dictionary<string, object>
        {
            { "class", "form-horizontal" },
            { "id", "contact-form" }
        };
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = "contactForm",
            AdditionalAttributes = additionalAttributes,
            InnerContent = builder =>
            {
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "type", "text");
                builder.AddAttribute(2, "placeholder", "Name");
                builder.CloseElement();
            }
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);

        var methodAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "method");
        Assert.Equal("post", methodAttribute.AttributeValue);

        var classAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "class");
        Assert.Equal("form-horizontal", classAttribute.AttributeValue);

        var inputElement = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Element && f.ElementName == "input");
        Assert.NotNull(inputElement.ElementName);
    }

    [Fact]
    public async Task RendersWithOnSubmitAndAdditionalAttributes()
    {
        // Arrange
        var additionalAttributes = new Dictionary<string, object>
        {
            { "class", "form-submit" },
            { "id", "submit-form" }
        };
        var rootComponent = new TestHandlerFormHostComponent
        {
            OnSubmit = EventCallback.Factory.Create(this, async () => await Task.CompletedTask),
            AdditionalAttributes = additionalAttributes,
            InnerContent = builder =>
            {
                builder.AddContent(0, "Form Content");
            }
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);

        var classAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "class");
        Assert.Equal("form-submit", classAttribute.AttributeValue);
    }

    [Fact]
    public async Task RendersWithMinimalParameters()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent();

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
        Assert.Equal("form", formElement.ElementName);

        var methodAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "method");
        Assert.NotNull(methodAttribute.AttributeName);
        Assert.Equal("post", methodAttribute.AttributeValue);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task RendersNestedContent()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "Nested content");
                builder.CloseElement();
            }
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task HandlesLargeNumberOfAttributes()
    {
        // Arrange
        var additionalAttributes = new Dictionary<string, object>();
        for (int i = 0; i < 20; i++)
        {
            additionalAttributes[$"data-attr-{i}"] = $"value-{i}";
        }
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = additionalAttributes
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task HandlesSpecialCharactersInFormName()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = "form-name_with.special-chars"
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task HandlesDynamicChildContent()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder =>
            {
                foreach (var item in items)
                {
                    builder.OpenElement(0, "div");
                    builder.AddContent(1, item);
                    builder.CloseElement();
                }
            }
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var divElements = frames.Where(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "div").ToArray();
        Assert.Equal(3, divElements.Length);
    }

    [Fact]
    public async Task HandlesComplexChildContent()
    {
        // Arrange
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder =>
            {
                builder.OpenElement(0, "fieldset");
                builder.OpenElement(1, "legend");
                builder.AddContent(2, "User Information");
                builder.CloseElement();

                builder.OpenElement(3, "div");
                builder.OpenElement(4, "label");
                builder.AddContent(5, "Name:");
                builder.CloseElement();
                builder.OpenElement(6, "input");
                builder.AddAttribute(7, "type", "text");
                builder.CloseElement();
                builder.CloseElement();

                builder.CloseElement();
            }
        };

        // Act
        var frames = await RenderAndGetFrames(rootComponent);

        // Assert
        var fieldsetElement = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Element && f.ElementName == "fieldset");
        var legendElement = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Element && f.ElementName == "legend");
        var inputElement = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Element && f.ElementName == "input");

        Assert.NotNull(fieldsetElement.ElementName);
        Assert.NotNull(legendElement.ElementName);
        Assert.NotNull(inputElement.ElementName);
    }

    #endregion

    #region Helper Methods

    private async Task<RenderTreeFrame[]> RenderAndGetFrames(TestHandlerFormHostComponent rootComponent)
    {
        var componentId = _testRenderer.AssignRootComponentId(rootComponent);
        await _testRenderer.RenderRootComponentAsync(componentId);

        var batch = _testRenderer.Batches.Single();
        return batch.ReferenceFrames;
    }

    #endregion

    #region Test Host Component

    private class TestHandlerFormHostComponent : ComponentBase
    {
        public RenderFragment? InnerContent { get; set; } = null;
        public string? FormName { get; set; } = null;
        public EventCallback OnSubmit { get; set; } = default;
        public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; } = null;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<HandlerForm>(0);
            builder.AddComponentParameter(1, "ChildContent", InnerContent);
            builder.AddComponentParameter(2, "FormName", FormName);
            builder.AddComponentParameter(3, "OnSubmit", OnSubmit);
            if (AdditionalAttributes != null)
            {
                builder.AddComponentParameter(4, "AdditionalAttributes", AdditionalAttributes);
            }
            builder.CloseComponent();
        }
    }

    #endregion
}
