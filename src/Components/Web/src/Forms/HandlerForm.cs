// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// Renders a lightweight form element that preserves native HTML form behavior
/// while enabling optional integration with Razor components.
/// </summary>
/// <remarks>
/// <para>This component does not support validation or <see cref="EditContext"/> integration.
/// For scenarios requiring validation, use <see cref="EditForm"/> instead.</para>
/// </remarks>
public class HandlerForm : ComponentBase
{
    // Cached delegate to avoid per-render allocations when OnSubmit is invoked
    private readonly Func<EventArgs, Task> _handleSubmitDelegate;

    public HandlerForm()
    {
        _handleSubmitDelegate = HandleSubmitAsync;
    }

    /// <summary>
    /// Gets or sets the content to be rendered inside the form element,
    /// typically including form controls such as input fields and buttons.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A callback that will be invoked when the form is submitted.
    /// </summary>
    /// <remarks>
    /// <para>If <see cref="OnSubmit"/> has a delegate assigned, the form's default browser
    /// submission is prevented and only the callback is invoked. This enables fully
    /// client-side form handling via Blazor without a page reload.</para>
    /// <para>If <see cref="OnSubmit"/> does not have a delegate assigned (the default),
    /// the form performs a standard native HTML POST to the server with method="post".
    /// No JavaScript interop is required for this mode.</para>
    /// </remarks>
    [Parameter] public EventCallback<EventArgs> OnSubmit { get; set; }

    /// <summary>
    /// Gets or sets the name attribute for the form element. This value is used to
    /// identify the form in the rendering tree and corresponds to the HTML name attribute.
    /// </summary>
    [Parameter] public string? FormName { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to apply to the form element. These allow
    /// consuming code to specify attributes such as <c>class</c>, <c>id</c>, <c>aria-*</c>,
    /// or <c>data-*</c> that will be rendered on the HTML form element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject] private AntiforgeryStateProvider AntiforgeryStateProvider { get; set; } = default!;

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        Debug.Assert(AntiforgeryStateProvider != null);

        builder.OpenElement(0, "form");

        int nextSequence = 1;
        if (OnSubmit.HasDelegate)
        {
            // EventCallback.Factory.Create is called per-render when transitioning from
            // no-handler to handler state. This is a minor allocation in hot paths but avoids
            // the complexity of tracking state transitions. The handler delegate is cached.
            builder.AddEventPreventDefaultAttribute(nextSequence++, "onsubmit", true);
            builder.AddAttribute(nextSequence++, "onsubmit",
               EventCallback.Factory.Create<EventArgs>(this, HandleSubmitAsync));
        }

        if (AdditionalAttributes is not null)
        {
            builder.AddMultipleAttributes(nextSequence++, AdditionalAttributes);
        }
        builder.AddAttribute(nextSequence++, "method", "post");
        if (!string.IsNullOrEmpty(FormName))
        {
            builder.AddNamedEvent("onsubmit", FormName);
        }

        builder.AddContent(nextSequence++, ChildContent);

        // Render antiforgery token in server-side contexts
        if (AntiforgeryStateProvider != null)
        {
            builder.OpenComponent<AntiforgeryToken>(nextSequence++);
            builder.CloseComponent();
        }

        builder.CloseElement();
    }

    /// <summary>
    /// Handles the form submission event and invokes the <see cref="OnSubmit"/> callback
    /// if one has been configured.
    /// </summary>
    private async Task HandleSubmitAsync(EventArgs args)
    {
        if (OnSubmit.HasDelegate)
        {
            await OnSubmit.InvokeAsync(args);
        }
    }
}
