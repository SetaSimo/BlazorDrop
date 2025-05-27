using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropList<T> : IAsyncDisposable, IDisposable
    {
        [Parameter]
        public Func<T, Task<T>> OnItemClick { get; set; }

        [Parameter]
        public Func<T, string> DisplaySelector { get; set; }

        private DotNetObjectReference<BlazorDropList<T>> _dotNetRef;

        private string GetDisplayValue(T item)
        {
            if (DisplaySelector == null)
            {
                return item.ToString();
            }

            return DisplaySelector(item);
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadPageAsync(CurrentPage);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);

                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerScrollHandler", _dotNetRef, _scrollContainerId, nameof(OnScrollToEndAsync));
            }
        }

        private async Task HandleItemSelectedAsync(T value)
        {
            if (OnItemClick == null)
            {
                Value = value;
            }
            else
            {
                ShowLoadingIndicator(true);
                Value = await OnItemClick.Invoke(value);
                ShowLoadingIndicator(false);
            }
        }

        private async Task UnregisterScrollHandlerAsync()
        {
            await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterScrollHandler", _scrollContainerId);
        }

        public void Dispose()
        {
        }

        public async ValueTask DisposeAsync()
        {
            if (_dotNetRef != null)
            {
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterScrollHandler", _scrollContainerId);

                _dotNetRef.Dispose();
            }
        }
    }
}