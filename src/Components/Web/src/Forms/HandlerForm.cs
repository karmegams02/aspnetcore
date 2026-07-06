// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    private readonly Func<EventArgs, Task> _handleSubmitDelegate; // Cache to avoid per-render allocations

    /// <summary>
    /// Constructs an instance of <see cref="HandlerForm"/>.
    /// </summary>
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
    /// <para>The form always performs a native HTML POST to the server with
    /// <c>method="post"</c> and includes an antiforgery token. The <see cref="OnSubmit"/>
    /// callback fires alongside the native submission; assigning a delegate does not on
    /// its own prevent the default browser behavior.</para>
    /// <para>To handle the form entirely in Blazor without a page reload, also set
    /// <see cref="PreventDefault"/> to <c>true</c>, or call <c>event.preventDefault()</c>
    /// from within the handler. The SSR/logout-style flow
    /// (<c>&lt;HandlerForm FormName="logout"&gt;</c> with no <see cref="OnSubmit"/>)
    /// requires nothing else.</para>
    /// </remarks>
    [Parameter] public EventCallback<EventArgs> OnSubmit { get; set; }

    /// <summary>
    /// Gets or sets the name attribute for the form element. This value is used to
    /// identify the form in the rendering tree and corresponds to the HTML name attribute.
    /// </summary>
    [Parameter] public string? FormName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the form's default browser submission
    /// is prevented. When <c>true</c>, the form will not perform its native POST to the
    /// server and handling is delegated to Blazor (typically via the
    /// <see cref="OnSubmit"/> callback).
    /// </summary>
    [Parameter] public bool PreventDefault { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to apply to the form element. These allow
    /// consuming code to specify attributes such as <c>class</c>, <c>id</c>, <c>aria-*</c>,
    /// or <c>data-*</c> that will be rendered on the HTML form element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "form");

        int nextSequence = 1;
        if (PreventDefault)
        {
            builder.AddEventPreventDefaultAttribute(nextSequence++, "onsubmit", true);
        }
        if (OnSubmit.HasDelegate)
        {
            builder.AddAttribute(nextSequence++, "onsubmit", _handleSubmitDelegate);
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

        builder.OpenComponent<AntiforgeryToken>(nextSequence++);
        builder.CloseComponent();

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
