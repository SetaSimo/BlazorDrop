using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace BlazorDrop.Components.Base
{
    public abstract class BaseLazyComponent : ComponentBase
    {
        [Parameter] public string Id { get; set; } = "bzd-" + Guid.NewGuid().ToString("N");

        [Parameter] public string? Class { get; set; }

        [Parameter] public string? Style { get; set; }

        [Parameter] public string? AriaLabel { get; set; }

        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    }
}
