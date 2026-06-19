// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.AspNetCore.Components.Rendering;

namespace Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// Renders a lightweight form element that preserves native HTML form behavior
/// while enabling optional integration with Razor components.
/// </summary>
public class HandlerForm : ComponentBase
{
    private readonly Func<Task> _handleSubmitDelegate; // Cache to avoid per-render allocations

    /// <summary>
    /// Constructs an instance of <see cref="HandlerForm"/>.
    /// </summary>
    public HandlerForm()
    {
        _handleSubmitDelegate = HandleSubmitAsync;
    }

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="HandlerForm"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A callback that will be invoked when the form is submitted.
    /// Only attached to the form when a handler is present.
    /// </summary>
    [Parameter] public EventCallback OnSubmit { get; set; }

    /// <summary>
    /// Gets or sets the name of the form. This is used to uniquely identify the form
    /// in the Blazor framework, equivalent to the @formname directive attribute.
    /// </summary>
    [Parameter] public string? FormName { get; set; }

    /// <summary>
    /// Captures unmatched HTML attributes and applies them to the rendered form element.
    /// Allows consumers to set custom attributes such as <c>class</c>, <c>id</c>, <c>aria-*</c>,
    /// <c>data-*</c>, and other standard HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject] private AntiforgeryStateProvider AntiforgeryStateProvider { get; set; } = default!;

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        Debug.Assert(AntiforgeryStateProvider != null);

        builder.OpenElement(0, "form");

        // Add method attribute
        builder.AddAttribute(1, "method", "post");

        // Track sequence for proper ordering
        int nextSequence = 2;

        // Only attach onsubmit handler if OnSubmit has a delegate
        if (OnSubmit.HasDelegate)
        {
            builder.AddAttribute(nextSequence++, "onsubmit", _handleSubmitDelegate);
        }

        // Add pass-through HTML attributes
        if (AdditionalAttributes is not null)
        {
            builder.AddMultipleAttributes(nextSequence++, AdditionalAttributes);
        }

        // Add form name attribute if specified (equivalent to @formname directive)
        // AddNamedEvent must be called while still in element context and before content
        if (!string.IsNullOrEmpty(FormName))
        {
            builder.AddNamedEvent("onsubmit", FormName);
        }

        // Render child content
        builder.AddContent(nextSequence++, ChildContent);

        // Render antiforgery token in server-side contexts
        // The AntiforgeryToken component will safely no-op if HttpContext is unavailable (e.g., WASM)
        if (AntiforgeryStateProvider != null)
        {
            builder.OpenComponent<AntiforgeryToken>(nextSequence++);
            builder.CloseComponent();
        }

        builder.CloseElement();
    }

    private async Task HandleSubmitAsync()
    {
        if (OnSubmit.HasDelegate)
        {
            await OnSubmit.InvokeAsync();
        }
    }
}
