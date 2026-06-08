// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Components.Web;

/// <summary>
/// This class handles toggle-related events.
/// </summary>

public class ToggleEventArgs : EventArgs
{
    /// <summary>
    /// ToggleEvent constructor.
    /// </summary>
    public ToggleEventArgs()
    {

    }

    /// <summary>
    /// Gets or sets the new state of the toggled element.
    /// </summary>
    [JsonPropertyName("newState")]
    public string NewState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old state of the toggled element.
    /// </summary>
    [JsonPropertyName("oldState")]
    public string OldState { get; set; } = string.Empty;
}
