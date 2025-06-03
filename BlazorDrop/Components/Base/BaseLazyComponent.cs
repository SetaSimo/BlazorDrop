using Microsoft.AspNetCore.Components;
using System;
namespace BlazorDrop.Components.Base
{
    public abstract class BaseLazyComponent : ComponentBase
    {
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public string Style { get; set; }
    }
}
