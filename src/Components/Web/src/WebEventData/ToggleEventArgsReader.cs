// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Text.Json;

namespace Microsoft.AspNetCore.Components.Web;

internal static class ToggleEventArgsReader
{
    private static readonly JsonEncodedText OldState = JsonEncodedText.Encode("oldState");
    private static readonly JsonEncodedText NewState = JsonEncodedText.Encode("newState");

    internal static ToggleEventArgs Read(JsonElement jsonElement)
    {
        var eventArgs = new ToggleEventArgs();
        foreach (var property in jsonElement.EnumerateObject())
        {
            if (property.NameEquals(OldState.EncodedUtf8Bytes))
            {
                eventArgs.OldState = property.Value.GetString() ?? string.Empty;
            }
            else if (property.NameEquals(NewState.EncodedUtf8Bytes))
            {
                eventArgs.NewState = property.Value.GetString() ?? string.Empty;
            }
            else
            {
                throw new JsonException($"Unknown property {property.Name}");
            }
        }
        return eventArgs;
    }
}
