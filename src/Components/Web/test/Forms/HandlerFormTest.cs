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
    private TestRenderer _testRenderer;

    public HandlerFormTest()
    {
        var services = new ServiceCollection();
        services.AddAntiforgery();
        services.AddLogging();
        services.AddSingleton<AntiforgeryStateProvider, DefaultAntiforgeryStateProvider>();
        _testRenderer = new(services.BuildServiceProvider());
    }

    [Fact]
    public async Task RendersFormElement_AsRoot()
    {
        var rootComponent = new TestHandlerFormHostComponent();

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        AssertFrame.Element(frames.Array[0], "form", subtreeLength: frames.Count, sequence: 0);
    }

    [Fact]
    public async Task RendersMethodPostAttribute()
    {
        var rootComponent = new TestHandlerFormHostComponent();

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        var methodAttribute = FindAttribute(frames, "method");
        AssertFrame.Attribute(methodAttribute, "method", "post");
    }

    [Theory]
    [InlineData("myform")]
    [InlineData("form-name_with.special-chars")]
    public async Task AddsNamedEventForFormName_WhenFormNameIsProvided(string formName)
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = formName
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        var namedEvent = FindNamedEvent(frames, "onsubmit");
        AssertFrame.NamedEvent(namedEvent, "onsubmit", formName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DoesNotAddNamedEvent_WhenFormNameIsNullOrEmpty(string? formName)
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = formName
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        Assert.Null(FindNamedEventOrDefault(frames));
    }

    [Fact]
    public async Task RendersAntiforgeryTokenComponent_Always()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            OnSubmit = EventCallback.Factory.Create<EventArgs>(this, args => Task.CompletedTask),
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        // Assert: AntiforgeryToken component frame is present with subtree length 1.
        var antiforgeryComponent = FindComponent<AntiforgeryToken>(frames);
        AssertFrame.Component<AntiforgeryToken>(antiforgeryComponent, subtreeLength: 1);
    }

    [Fact]
    public async Task AddsOnSubmitAttribute_WhenOnSubmitHasDelegate()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            OnSubmit = EventCallback.Factory.Create<EventArgs>(this, args => Task.CompletedTask),
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);
        var onsubmitAttribute = FindAttribute(frames, "onsubmit");
        AssertFrame.Attribute(onsubmitAttribute, "onsubmit", typeof(Func<EventArgs, Task>));
    }

    [Fact]
    public async Task DoesNotAddOnSubmitAttribute_WhenOnSubmitHasNoDelegate()
    {
        var rootComponent = new TestHandlerFormHostComponent();

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        Assert.Null(FindAttributeOrDefault(frames, "onsubmit"));
    }

    [Fact]
    public async Task AddsPreventDefaultAttribute_WhenPreventDefaultIsTrue()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            PreventDefault = true,
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        var preventDefault = FindAttributeOrDefault(frames, "__internal_preventDefault_onsubmit");
        Assert.NotNull(preventDefault);
    }

    [Fact]
    public async Task DoesNotAddPreventDefaultAttribute_WhenPreventDefaultIsFalse()
    {
        var rootComponent = new TestHandlerFormHostComponent();

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        Assert.Null(FindAttributeOrDefault(frames, "__internal_preventDefault_onsubmit"));
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
            AdditionalAttributes = additionalAttributes,
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        var attribute = FindAttribute(frames, attributeName);
        AssertFrame.Attribute(attribute, attributeName, expectedValue);
    }

    [Fact]
    public async Task MethodPostAttribute_WinsOverAdditionalAttributesMethod()
    {
        var additionalAttributes = new Dictionary<string, object>
        {
            { "method", "dialog" },
        };
        var rootComponent = new TestHandlerFormHostComponent
        {
            AdditionalAttributes = additionalAttributes,
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);
        var methodAttributes = frames.AsEnumerable()
            .Where(f => f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "method")
            .ToArray();

        Assert.NotEmpty(methodAttributes);
        Assert.Equal("post", methodAttributes[^1].AttributeValue);
    }

    [Fact]
    public async Task RendersChildContent()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder =>
            {
                builder.AddContent(0, "Form Content");
            },
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        var textFrame = FindText(frames, "Form Content");
        AssertFrame.Text(textFrame, "Form Content");
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
            },
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        var inputElement = FindElement(frames, "input");
        var buttonElement = FindElement(frames, "button");
        AssertFrame.Element(inputElement, "input", subtreeLength: 2);
        AssertFrame.Element(buttonElement, "button", subtreeLength: 2);

        AssertFrame.Attribute(FindAttribute(frames, "type"), "type", "text");
        AssertFrame.Text(FindText(frames, "Submit"), "Submit");
    }

    [Fact]
    public async Task RendersComplexChildContent()
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
            },
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        AssertFrame.Element(FindElement(frames, "fieldset"), "fieldset", subtreeLength: 8);
        AssertFrame.Element(FindElement(frames, "legend"), "legend", subtreeLength: 2);
        AssertFrame.Element(FindElement(frames, "div"), "div", subtreeLength: 5);
        AssertFrame.Element(FindElement(frames, "label"), "label", subtreeLength: 2);
        AssertFrame.Element(FindElement(frames, "input"), "input", subtreeLength: 2);

        AssertFrame.Text(FindText(frames, "User Information"), "User Information");
        AssertFrame.Text(FindText(frames, "Name:"), "Name:");
    }

    [Fact]
    public async Task RendersAllFrames_WithAllBranchesDisabled()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            InnerContent = builder => builder.AddContent(0, "Body"),
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        Assert.Collection(frames.AsEnumerable(),
            frame => AssertFrame.Element(frame, "form", subtreeLength: frames.Count, sequence: 0),
            frame => AssertFrame.Attribute(frame, "method", "post", sequence: 1),
            frame => AssertFrame.Region(frame, subtreeLength: 2, sequence: 2),
            frame => AssertFrame.Text(frame, "Body", sequence: 0),
            frame => AssertFrame.Component<AntiforgeryToken>(frame, subtreeLength: 1, sequence: 3));
    }

    [Fact]
    public async Task RendersAllFrames_WithOnSubmit()
    {
        var rootComponent = new TestHandlerFormHostComponent
        {
            OnSubmit = EventCallback.Factory.Create<EventArgs>(this, args => Task.CompletedTask),
            InnerContent = builder => builder.AddContent(0, "Body"),
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        Assert.Collection(frames.AsEnumerable(),
            frame => AssertFrame.Element(frame, "form", subtreeLength: frames.Count, sequence: 0),
            frame => AssertFrame.Attribute(frame, "onsubmit", typeof(Func<EventArgs, Task>), sequence: 1),
            frame => AssertFrame.Attribute(frame, "method", "post", sequence: 2),
            frame => AssertFrame.Region(frame, subtreeLength: 2, sequence: 3),
            frame => AssertFrame.Text(frame, "Body", sequence: 0),
            frame => AssertFrame.Component<AntiforgeryToken>(frame, subtreeLength: 1, sequence: 4));
    }

    [Fact]
    public async Task RendersAllFrames_WithFormName()
    {

        var rootComponent = new TestHandlerFormHostComponent
        {
            FormName = "contactForm",
            InnerContent = builder => builder.AddContent(0, "Body"),
        };

        var frames = await RenderAndGetHandlerFormFramesAsync(rootComponent);

        Assert.Collection(frames.AsEnumerable(),
            frame => AssertFrame.Element(frame, "form", subtreeLength: frames.Count, sequence: 0),
            frame => AssertFrame.Attribute(frame, "method", "post", sequence: 1),
            frame => AssertFrame.NamedEvent(frame, "onsubmit", "contactForm"),
            frame => AssertFrame.Region(frame, subtreeLength: 2, sequence: 2),
            frame => AssertFrame.Text(frame, "Body", sequence: 0),
            frame => AssertFrame.Component<AntiforgeryToken>(frame, subtreeLength: 1, sequence: 3));
    }

    private async Task<ArrayRange<RenderTreeFrame>> RenderAndGetHandlerFormFramesAsync(TestHandlerFormHostComponent rootComponent)
    {
        var componentId = _testRenderer.AssignRootComponentId(rootComponent);
        await _testRenderer.RenderRootComponentAsync(componentId);

        var handlerFormComponentId = _testRenderer.Batches
            .SelectMany(b => b.ReferenceFrames.AsEnumerable())
            .Where(f => f.FrameType == RenderTreeFrameType.Component)
            .Where(f => f.Component is HandlerForm)
            .Select(f => f.ComponentId)
            .Single();

        return _testRenderer.GetCurrentRenderTreeFrames(handlerFormComponentId).Clone();
    }

    private static RenderTreeFrame FindAttribute(ArrayRange<RenderTreeFrame> frames, string attributeName)
        => FindAttributeOrDefault(frames, attributeName)
            ?? throw new Xunit.Sdk.XunitException($"No attribute named '{attributeName}' was found.");

    private static RenderTreeFrame? FindAttributeOrDefault(ArrayRange<RenderTreeFrame> frames, string attributeName)
    {
        foreach (var f in frames.AsEnumerable())
        {
            if (f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == attributeName)
            {
                return f;
            }
        }
        return null;
    }

    private static RenderTreeFrame FindElement(ArrayRange<RenderTreeFrame> frames, string elementName)
        => FindElementOrDefault(frames, elementName)
            ?? throw new Xunit.Sdk.XunitException($"No element named '{elementName}' was found.");

    private static RenderTreeFrame? FindElementOrDefault(ArrayRange<RenderTreeFrame> frames, string elementName)
    {
        foreach (var f in frames.AsEnumerable())
        {
            if (f.FrameType == RenderTreeFrameType.Element && f.ElementName == elementName)
            {
                return f;
            }
        }
        return null;
    }

    private static RenderTreeFrame FindText(ArrayRange<RenderTreeFrame> frames, string textContent)
    {
        foreach (var f in frames.AsEnumerable())
        {
            if (f.FrameType == RenderTreeFrameType.Text && f.TextContent == textContent)
            {
                return f;
            }
        }
        throw new Xunit.Sdk.XunitException($"No text frame with content '{textContent}' was found.");
    }

    private static RenderTreeFrame FindNamedEvent(ArrayRange<RenderTreeFrame> frames, string eventType)
        => FindNamedEventOrDefault(frames, eventType)
            ?? throw new Xunit.Sdk.XunitException($"No named event of type '{eventType}' was found.");

    private static RenderTreeFrame? FindNamedEventOrDefault(ArrayRange<RenderTreeFrame> frames, string? eventType = null)
    {
        foreach (var f in frames.AsEnumerable())
        {
            if (f.FrameType != RenderTreeFrameType.NamedEvent)
            {
                continue;
            }
            if (eventType is null || f.NamedEventType == eventType)
            {
                return f;
            }
        }
        return null;
    }

    private static RenderTreeFrame FindComponent<T>(ArrayRange<RenderTreeFrame> frames) where T : IComponent
    {
        foreach (var f in frames.AsEnumerable())
        {
            if (f.FrameType == RenderTreeFrameType.Component && f.ComponentType == typeof(T))
            {
                return f;
            }
        }
        throw new Xunit.Sdk.XunitException($"No component of type {typeof(T).Name} was found.");
    }

    private class TestHandlerFormHostComponent : ComponentBase
    {
        public RenderFragment? InnerContent { get; set; }
        public string? FormName { get; set; }
        public EventCallback<EventArgs> OnSubmit { get; set; }
        public bool PreventDefault { get; set; }
        public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<HandlerForm>(0);
            builder.AddComponentParameter(1, "ChildContent", InnerContent);
            builder.AddComponentParameter(2, "FormName", FormName);
            builder.AddComponentParameter(3, "OnSubmit", OnSubmit);
            builder.AddComponentParameter(4, "PreventDefault", PreventDefault);
            if (AdditionalAttributes != null)
            {
                builder.AddComponentParameter(5, "AdditionalAttributes", AdditionalAttributes);
            }
            builder.CloseComponent();
        }
    }
}
