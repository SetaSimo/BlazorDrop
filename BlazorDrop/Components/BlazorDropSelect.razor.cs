using BlazorDrop.Components.Base.Select;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropSelect<T> : BaseLazyInputWithSelect<T>, IAsyncDisposable
    {
        [Parameter]
        public T Value { get; set; }

        private DotNetObjectReference<BlazorDropSelect<T>> _dotNetRef;

        protected override async Task OnInitializedAsync()
        {
            await LoadPageAsync(CurrentPage);
            UpdateSearchTextAfterSelect(Value);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Disabled is false)
            {
                _dotNetRef = DotNetObjectReference.Create(this);

                await RegisterInputHandlerAsync(_inputSelectorId, _inputSelectorId, _dotNetRef);
            }

            if (_isDropdownOpen && _isScrollHandlerAttached is false && Disabled is false)
            {
                await RegisterScrollHandlerAsync(_scrollContainerId, nameof(OnScrollToEndAsync), _dotNetRef);
            }
        }

        protected override async Task HandleItemSelectedAsync(T value)
        {
            await SetLoadingStateAsync(true);

            if (OnItemClickAsync == null)
            {
                Value = value;
            }
            else
            {
                Value = await OnItemClickAsync.Invoke(value);
            }

            UpdateSearchTextAfterSelect(Value);

            await SetLoadingStateAsync(false);
        }

        private void UpdateSearchTextAfterSelect(T value)
        {
            if (value == null)
                return;

            _searchText = GetDisplayValue(value);
        }

        protected override bool IsItemSelected(T item)
        {
            return EqualityComparer<T>.Default.Equals(item, Value);
        }

        public async ValueTask DisposeAsync()
        {
            if (_dotNetRef != null)
            {
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterClickOutsideHandler", _inputSelectorId);
                await UnregisterScrollHandlerAsync(_scrollSelectorId);

                _dotNetRef.Dispose();
            }
        }
    }
}