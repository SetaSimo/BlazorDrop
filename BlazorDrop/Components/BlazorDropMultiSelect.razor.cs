using BlazorDrop.Components.Base.Select;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropMultiSelect<T> : BaseLazyInputWithSelect<T>, IAsyncDisposable
    {
        private DotNetObjectReference<BlazorDropMultiSelect<T>> _dotNetRef;

        public List<T> SelectedValues { get; set; } = new List<T>();

        protected override async Task OnInitializedAsync()
        {
            await LoadPageAsync(CurrentPage);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Disabled is false)
            {
                _dotNetRef = DotNetObjectReference.Create(this);

                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.initInputHandler", _dotNetRef, _inputSelectorId, UpdateSearchDelayInMilliseconds);
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerClickOutsideHandler", _dotNetRef, Id);
            }

            if (_isDropdownOpen && _isScrollHandlerAttached is false && Disabled is false)
            {
                await RegisterScrollHandlerAsync(_scrollContainerId, nameof(OnScrollToEndAsync), _dotNetRef);
            }
        }

        protected override async Task HandleItemSelectedAsync(T value)
        {
            await SetLoadingStateAsync(true);

            if (OnItemClickAsync != null)
            {
                value = await OnItemClickAsync.Invoke(value);
            }

            var wasItemAdded = SelectedValues.Any(v => EqualityComparer<T>.Default.Equals(v, value));

            if (wasItemAdded)
            {
                SelectedValues.Remove(value);
            }
            else
            {
                SelectedValues.Add(value);
            }

            await SetLoadingStateAsync(false);
        }

        protected override bool IsItemSelected(T item)
        {
            return SelectedValues.Contains(item);
        }

        public async ValueTask DisposeAsync()
        {
            if (_dotNetRef != null)
            {
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterClickOutsideHandler", _inputSelectorId);
                await UnregisterScrollHandlerAsync(_scrollContainerId);

                _dotNetRef.Dispose();
            }
        }
    }
}