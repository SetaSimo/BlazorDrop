using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
namespace BlazorDrop.Components.Base
{
    public abstract class BaseLazyComponent : ComponentBase
    {
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Parameter]
        public string Class { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected bool _isLoading = false;
    }
}
