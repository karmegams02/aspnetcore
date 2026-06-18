// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// Renders a lightweight form element that preserves native HTML form behavior
/// while enabling optional integration with Razor components.
/// </summary>
public class LogoutForm : ComponentBase
{
    private readonly Func<Task> _handleSubmitDelegate; // Cache to avoid per-render allocations

    /// <summary>
    /// Constructs an instance of <see cref="LogoutForm"/>.
    /// </summary>
    public LogoutForm()
    {
        _handleSubmitDelegate = HandleSubmitAsync;
    }

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="LogoutForm"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A callback that will be invoked when the form is submitted.
    /// Only attached to the form when a handler is present.
    /// </summary>
    [Parameter] public EventCallback<FormSubmitEventArgs> OnSubmit { get; set; }

    /// <summary>
    /// Specifies the HTTP method for the form. Defaults to "post".
    /// </summary>
    [Parameter] public string Method { get; set; } = "post";

    /// <summary>
    /// Specifies the URL to which the form will be submitted.
    /// If not specified, the form submits to the current URL.
    /// </summary>
    [Parameter] public string? Action { get; set; }

    /// <summary>
    /// When true, prevents the default form submission behavior,
    /// allowing only Blazor event handling via OnSubmit.
    /// </summary>
    [Parameter] public bool PreventDefault { get; set; }

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

    [Inject] private IServiceProvider Services { get; set; } = default!;

    /// <inheritdoc />
#pragma warning disable RS0016 // Symbol 'BuildRenderTree' is not part of the declared API
    protected override void BuildRenderTree(RenderTreeBuilder builder)
#pragma warning restore RS0016
    {
        Debug.Assert(Services != null);

        builder.OpenElement(0, "form");

        // Add method attribute
        builder.AddAttribute(1, "method", Method);

        // Add action attribute if specified
        if (!string.IsNullOrEmpty(Action))
        {
            builder.AddAttribute(2, "action", Action);
        }

        // Add form name attribute if specified (equivalent to @formname directive)
        if (!string.IsNullOrEmpty(FormName))
        {
            builder.AddAttribute(3, "name", FormName);
        }

        // Only attach onsubmit handler if OnSubmit has a delegate or PreventDefault is true
        int nextSequence = 4;
        if (OnSubmit.HasDelegate || PreventDefault)
        {
            builder.AddAttribute(nextSequence++, "onsubmit", _handleSubmitDelegate);
        }

        // Add pass-through HTML attributes
        if (AdditionalAttributes is not null)
        {
            builder.AddMultipleAttributes(nextSequence++, AdditionalAttributes);
        }

        // Render child content
        builder.AddContent(nextSequence++, ChildContent);

        // Render antiforgery token in server-side contexts
        // The AntiforgeryToken component will safely no-op if HttpContext is unavailable (e.g., WASM)
        var antiforgeryStateProvider = Services.GetService<AntiforgeryStateProvider>();
        if (antiforgeryStateProvider != null)
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
            await OnSubmit.InvokeAsync(new FormSubmitEventArgs());
        }
    }
}
