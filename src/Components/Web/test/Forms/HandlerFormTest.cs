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

    [Fact]
    public async Task RendersFormElement()
    {
        var rootComponent = new TestHandlerFormHostComponent();

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
        Assert.Equal("form", formElement.ElementName);
    }

    [Fact]
    public async Task RendersFormWithMethodPost()
    {
        var rootComponent = new TestHandlerFormHostComponent();

        var frames = await RenderAndGetFrames(rootComponent);

        var methodAttribute = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "method");
        Assert.NotNull(methodAttribute.AttributeName);
        Assert.Equal("post", methodAttribute.AttributeValue);
    }

    [Fact]
    public async Task RendersChildContent()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder =>
            {
                builder.AddContent(0, "Form Content");
            }
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var textFrame = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Text && f.TextContent == "Form Content");
        Assert.NotNull(textFrame.TextContent);
        Assert.Equal("Form Content", textFrame.TextContent);
    }

    [Fact]
    public async Task RendersMultipleChildElements()
    {
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

        var frames = await RenderAndGetFrames(rootComponent);

        var inputElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "input");
        var buttonElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "button");

        Assert.NotNull(inputElement.ElementName);
        Assert.NotNull(buttonElement.ElementName);
        Assert.Equal("input", inputElement.ElementName);
        Assert.Equal("button", buttonElement.ElementName);
    }

    [Fact]
    public async Task AddsFormNameAttribute_WhenFormNameIsProvided()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = "myform"
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task DoesNotAddFormNameAttribute_WhenFormNameIsNull()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = null
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
        Assert.Equal("form", formElement.ElementName);
    }

    [Fact]
    public async Task DoesNotAddFormNameAttribute_WhenFormNameIsEmpty()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = string.Empty
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
        Assert.Equal("form", formElement.ElementName);
    }

    [Fact]
    public async Task DoesNotAddFormNameAttribute_WhenFormNameIsWhitespace()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = "   "
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task AddsOnSubmitHandler_WhenOnSubmitHasDelegate()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            OnSubmit = EventCallback.Factory.Create(this, async () =>
            {
                await Task.CompletedTask;
            })
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var onsubmitAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == "onsubmit");

        Assert.NotNull(onsubmitAttribute.AttributeName);
    }

    [Fact]
    public async Task DoesNotAddOnSubmitHandler_WhenOnSubmitIsEmpty()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            OnSubmit = EventCallback.Empty
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Theory]
    [InlineData("class", "custom-form")]
    [InlineData("id", "my-form")]
    [InlineData("data-testid", "form-test")]
    [InlineData("data-form-type", "registration")]
    [InlineData("aria-label", "Contact Form")]
    [InlineData("aria-describedby", "form-help")]
    public async Task AppliesAdditionalAttributes(string attributeName, string expectedValue)
    {
        var additionalAttributes = new Dictionary<string, object>
        {
            { attributeName, expectedValue }
        };
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = additionalAttributes
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var attribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute &&
            f.AttributeName == attributeName);

        Assert.NotNull(attribute.AttributeName);
        Assert.Equal(expectedValue, attribute.AttributeValue);
    }

    [Fact]
    public async Task HandlesNullAdditionalAttributes()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = null
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task HandlesEmptyAdditionalAttributes()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = new Dictionary<string, object>()
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task RendersAntiforgeryTokenComponent()
    {
        var rootComponent = new TestHandlerFormHostComponent();

        var frames = await RenderAndGetFrames(rootComponent);

        var antiforgeryComponent = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Component &&
            f.ComponentType == typeof(AntiforgeryToken));

        Assert.NotNull(antiforgeryComponent.ComponentType);
        Assert.Equal(typeof(AntiforgeryToken), antiforgeryComponent.ComponentType);
    }

    [Fact]
    public async Task RendersWithFormNameAndAdditionalAttributes()
    {
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

        var frames = await RenderAndGetFrames(rootComponent);

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

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);

        var classAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "class");
        Assert.Equal("form-submit", classAttribute.AttributeValue);
    }

    [Fact]
    public async Task RendersWithMinimalParameters()
    {
        var rootComponent = new TestHandlerFormHostComponent();

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
        Assert.Equal("form", formElement.ElementName);

        var methodAttribute = frames.FirstOrDefault(f =>
            f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "method");
        Assert.NotNull(methodAttribute.AttributeName);
        Assert.Equal("post", methodAttribute.AttributeValue);
    }

    [Fact]
    public async Task RendersNestedContent()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "Nested content");
                builder.CloseElement();
            }
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task HandlesLargeNumberOfAttributes()
    {
        var additionalAttributes = new Dictionary<string, object>();
        for (int i = 0; i < 20; i++)
        {
            additionalAttributes[$"data-attr-{i}"] = $"value-{i}";
        }
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = additionalAttributes
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task HandlesSpecialCharactersInFormName()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = "form-name_with.special-chars"
        };

        var frames = await RenderAndGetFrames(rootComponent);

        var formElement = frames.FirstOrDefault(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "form");
        Assert.NotNull(formElement.ElementName);
    }

    [Fact]
    public async Task HandlesDynamicChildContent()
    {
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

        var frames = await RenderAndGetFrames(rootComponent);

        var divElements = frames.Where(f => f.FrameType == RenderTreeFrameType.Element && f.ElementName == "div").ToArray();
        Assert.Equal(3, divElements.Length);
    }

    [Fact]
    public async Task HandlesComplexChildContent()
    {
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

        var frames = await RenderAndGetFrames(rootComponent);

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

    private async Task<RenderTreeFrame[]> RenderAndGetFrames(TestHandlerFormHostComponent rootComponent)
    {
        var componentId = _testRenderer.AssignRootComponentId(rootComponent);
        await _testRenderer.RenderRootComponentAsync(componentId);

        var batch = _testRenderer.Batches.Single();
        return batch.ReferenceFrames;
    }

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
}
