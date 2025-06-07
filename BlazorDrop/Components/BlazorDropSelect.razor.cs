using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropSelect<T> : IAsyncDisposable
    {
        [Parameter]
        public T Value { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CreateDotNetRef();
            await LoadPageAsync(CurrentPage);
            UpdateSearchTextAfterSelect(Value);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Disabled is false)
            {
                await RegisterInputHandlerAsync(_inputSelectorId, _inputSelectorId);
            }

            if (_isDropdownOpen && _isScrollHandlerAttached is false && Disabled is false)
            {
                await RegisterScrollHandlerAsync(_scrollContainerId, nameof(OnScrollToEndAsync), DotNetRef);
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
            if (DotNetRef != null)
            {
                await UnregisterClickOutsideHandler(_inputSelectorId);
                await UnregisterScrollHandlerAsync(_scrollSelectorId);

                DotNetRef.Dispose();
            }
        }
    }
}