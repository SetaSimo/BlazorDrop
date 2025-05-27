using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropSelect<T> : IAsyncDisposable, IDisposable
    {
        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public int UpdateSearchDelayInMilliseconds { get; set; } = 1000;

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public Func<string, Task<IEnumerable<T>>> OnSearchAsync { get; set; }

        private DotNetObjectReference<BlazorDropSelect<T>> _dotNetRef;

        private Guid _inputSelectorId = Guid.NewGuid();

        private string _searchText = string.Empty;

        private bool _isDropdownOpen = false;

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

                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.initInputHandler", _dotNetRef, _inputSelectorId, UpdateSearchDelayInMilliseconds);
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerClickOutsideHandler", _dotNetRef, _inputSelectorId);
            }

            if (_isDropdownOpen && _isScrollHandlerAttached is false && Disabled is false)
            {
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerScrollHandler", _dotNetRef, _scrollContainerId, nameof(OnScrollToEndAsync));
                _isScrollHandlerAttached = true;
            }
        }

        [JSInvokable]
        public async Task UpdateSearchListAfterInputAsync()
        {
            if (OnSearchAsync == null)
            {
                throw new ArgumentException($"{nameof(OnSearchAsync)} is null");
            }

            if (_isDropdownOpen is false)
                await OpenDropdownAsync();

            _hasLoadedAllItems = false;

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                await ResetSearchAsync();
                return;
            }

            await SearchWithFilterAsync();
        }

        [JSInvokable]
        public async Task CloseDropdownAsync()
        {
            _isDropdownOpen = false;
            _isScrollHandlerAttached = false;
            await UnregisterScrollHandlerAsync();
            StateHasChanged();
        }

        private async Task OpenDropdownAsync()
        {
            if (Disabled)
                return;

            _isDropdownOpen = true;
        }

        protected override async Task HandleItemSelectedAsync(T value)
        {
            await base.HandleItemSelectedAsync(value);

            UpdateSearchTextAfterSelect(Value);
        }

        private void UpdateSearchTextAfterSelect(T value)
        {
            if (value == null)
                return;

            _searchText = GetDisplayValue(value);
        }

        private async Task ResetSearchAsync()
        {
            CurrentPage = 0;
            await LoadPageAsync(CurrentPage);
            StateHasChanged();
        }

        private async Task SearchWithFilterAsync()
        {
            ShowLoadingProgress(true);
            await UnregisterScrollHandlerAsync();

            var newItems = await OnSearchAsync(_searchText);
            Items = newItems.ToList();

            ShowLoadingProgress(false);
        }

        public void Dispose()
        {
        }

        public async ValueTask DisposeAsync()
        {
            if (_dotNetRef != null)
            {
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterClickOutsideHandler", _inputSelectorId);
                await UnregisterScrollHandlerAsync();

                _dotNetRef.Dispose();
            }
        }
    }
}