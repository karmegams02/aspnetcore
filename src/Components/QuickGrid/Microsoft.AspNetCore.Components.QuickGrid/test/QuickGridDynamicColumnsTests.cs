// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Xunit;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

public class QuickGridDynamicColumnsTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void Should_RenderDynamicColumns_When_ColumnsProvided()
    {
        var columns = new List<DynamicColumn<TestItem>>
        {
            new DynamicColumn<TestItem>
            {
                Title = "PropertyCol",
                Property = (Expression<Func<TestItem, object?>>)(x => x.Name),
                Visible = true
            },
            new DynamicColumn<TestItem>
            {
                Title = "TemplateCol",
                Template = builder => builder.AddContent(0, "TemplateContent"),
                Visible = true
            }
        };

        var items = new List<TestItem>
        {
            new TestItem { Name = "A", Value = 1 }
        }.AsQueryable();

        var renderer = CreateRenderer();
        var testComponent = new DynamicColumnsTestComponent(columns, items);
        var componentId = renderer.AssignRootComponentId(testComponent);
        renderer.RenderRootComponent(componentId);

        var rendered = GetRenderedText(renderer);
        Assert.Contains("PropertyCol", rendered);
        Assert.Contains("TemplateCol", rendered);
        Assert.Contains("A", rendered);
        Assert.Contains("TemplateContent", rendered);
    }

    [Fact]
    public void Should_NotRenderInvisibleDynamicColumns()
    {
        var columns = new List<DynamicColumn<TestItem>>
        {
            new DynamicColumn<TestItem> { Title = "VisibleCol", Property = (Expression<Func<TestItem, object?>>)(x => x.Name), Visible = true },
            new DynamicColumn<TestItem> { Title = "HiddenCol", Property = (Expression<Func<TestItem, object?>>)(x => x.Value), Visible = false }
        };

        var items = new List<TestItem>
        {
            new TestItem { Name = "A", Value = 1 }
        }.AsQueryable();

        var renderer = CreateRenderer();
        var testComponent = new DynamicColumnsTestComponent(columns, items);
        var componentId = renderer.AssignRootComponentId(testComponent);
        renderer.RenderRootComponent(componentId);

        var rendered = GetRenderedText(renderer);
        Assert.Contains("VisibleCol", rendered);
        Assert.DoesNotContain("HiddenCol", rendered);
    }

    [Fact]
    public void Should_RenderStaticAndDynamicColumnsTogether()
    {
        var columns = new List<DynamicColumn<TestItem>>
        {
            new DynamicColumn<TestItem> { Title = "DynamicCol", Property = (Expression<Func<TestItem, object?>>)(x => x.Value), Visible = true }
        };

        var items = new List<TestItem>
        {
            new TestItem { Name = "A", Value = 1 }
        }.AsQueryable();

        var renderer = CreateRenderer();
        var testComponent = new StaticAndDynamicColumnsTestComponent(columns, items);
        var componentId = renderer.AssignRootComponentId(testComponent);
        renderer.RenderRootComponent(componentId);

        var rendered = GetRenderedText(renderer);
        Assert.Contains("StaticCol", rendered);
        Assert.Contains("DynamicCol", rendered);
    }

    [Fact]
    public void Should_HandleNullColumns_Gracefully()
    {
        var items = new List<TestItem>
        {
            new TestItem { Name = "A", Value = 1 }
        }.AsQueryable();

        var renderer = CreateRenderer();
        var testComponent = new DynamicColumnsTestComponent(null, items);
        var componentId = renderer.AssignRootComponentId(testComponent);
        renderer.RenderRootComponent(componentId);

        var rendered = GetRenderedText(renderer);
        // With no columns, the grid renders the table shell but no cell content. We only
        // assert that no dynamic column title leaks into the output.
        Assert.DoesNotContain("DynamicCol", rendered);
        Assert.DoesNotContain("PropertyCol", rendered);
        Assert.DoesNotContain("TemplateCol", rendered);
    }

    [Fact]
    public void Should_HandleEmptyColumns_Gracefully()
    {
        var items = new List<TestItem>
        {
            new TestItem { Name = "A", Value = 1 }
        }.AsQueryable();

        var renderer = CreateRenderer();
        var testComponent = new DynamicColumnsTestComponent(new List<DynamicColumn<TestItem>>(), items);
        var componentId = renderer.AssignRootComponentId(testComponent);
        renderer.RenderRootComponent(componentId);

        var rendered = GetRenderedText(renderer);
        // With no columns, the grid renders the table shell but no cell content. We only
        // assert that no dynamic column title leaks into the output.
        Assert.DoesNotContain("DynamicCol", rendered);
        Assert.DoesNotContain("PropertyCol", rendered);
        Assert.DoesNotContain("TemplateCol", rendered);
    }

    private static TestRenderer CreateRenderer()
    {
        var moduleLoadCompletion = new TaskCompletionSource();
        moduleLoadCompletion.SetResult();
        var testJsRuntime = new TestJsRuntime(moduleLoadCompletion, new TaskCompletionSource());
        var services = new ServiceCollection()
            .AddSingleton<IJSRuntime>(testJsRuntime)
            .AddSingleton<NavigationManager, TestNavigationManager>()
            .BuildServiceProvider();
        return new TestRenderer(services);
    }

    private static string GetRenderedText(TestRenderer renderer)
    {
        var lastBatch = renderer.Batches.LastOrDefault();
        Assert.NotNull(lastBatch);

        var builder = new System.Text.StringBuilder();
        foreach (var frame in lastBatch.ReferenceFrames)
        {
            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Text:
                    builder.Append(frame.TextContent);
                    break;
                case RenderTreeFrameType.Markup:
                    builder.Append(frame.MarkupContent);
                    break;
            }
        }
        return builder.ToString();
    }

    private class DynamicColumnsTestComponent : ComponentBase
    {
        private readonly IEnumerable<DynamicColumn<TestItem>>? _columns;
        private readonly IQueryable<TestItem> _items;

        public DynamicColumnsTestComponent(IEnumerable<DynamicColumn<TestItem>>? columns, IQueryable<TestItem> items)
        {
            _columns = columns;
            _items = items;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<QuickGrid<TestItem>>(0);
            builder.AddAttribute(1, "Items", _items);
            builder.AddAttribute(2, "Columns", _columns);
            builder.CloseComponent();
        }
    }

    private class StaticAndDynamicColumnsTestComponent : ComponentBase
    {
        private readonly IEnumerable<DynamicColumn<TestItem>> _columns;
        private readonly IQueryable<TestItem> _items;

        public StaticAndDynamicColumnsTestComponent(IEnumerable<DynamicColumn<TestItem>> columns, IQueryable<TestItem> items)
        {
            _columns = columns;
            _items = items;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<QuickGrid<TestItem>>(0);
            builder.AddAttribute(1, "Items", _items);
            builder.AddAttribute(2, "Columns", _columns);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b =>
            {
                b.OpenComponent<PropertyColumn<TestItem, string>>(0);
                b.AddAttribute(1, "Title", "StaticCol");
                b.AddAttribute(2, "Property", (Expression<Func<TestItem, string>>)(x => x.Name));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        }
    }
}
