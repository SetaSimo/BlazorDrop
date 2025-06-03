using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropSelect<T> : IAsyncDisposable
    {
        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public int UpdateSearchDelayInMilliseconds { get; set; } = 1000;

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public Func<string, Task<IEnumerable<T>>> OnSearchTextChangedAsync { get; set; }

        private DotNetObjectReference<BlazorDropSelect<T>> _dotNetRef;

        private string _inputSelectorId = Guid.NewGuid().ToString();
        private string _scrollSelectorId = Guid.NewGuid().ToString();

        private string _searchText = string.Empty;

        private bool _isDropdownOpen = false;

        protected override async Task OnInitializedAsync()
        {
            await SetLoadingStateAsync(true);

            await LoadPageAsync(CurrentPage);
            UpdateSearchTextAfterSelect(Value);

            await SetLoadingStateAsync(false);
            _isFirstLoad = false;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Disabled is false)
            {
                _dotNetRef = DotNetObjectReference.Create(this);

                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.initInputHandler", _dotNetRef, _inputSelectorId, UpdateSearchDelayInMilliseconds);
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerClickOutsideHandler", _dotNetRef, _inputSelectorId);
            }
        }

        [JSInvokable]
        public async Task UpdateSearchListAfterInputAsync()
        {
            if (OnSearchTextChangedAsync == null)
            {
                throw new ArgumentException($"{nameof(OnSearchTextChangedAsync)} is null");
            }

            if (_isDropdownOpen is false)
            {
                await OpenDropdownAsync();
            }

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
            await UnregisterScrollHandlerAsync(_scrollSelectorId);
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

        private async Task OnLoadingStateChangedAsync(bool isLoading)
        {
            _isLoading = isLoading;
            if (OnLoadingStateChanged.HasDelegate)
            {
                await OnLoadingStateChanged.InvokeAsync(isLoading);
            }
        }

        private async Task SearchWithFilterAsync()
        {
            await SetLoadingStateAsync(true);
            await UnregisterScrollHandlerAsync(_scrollSelectorId);

            var newItems = await OnSearchTextChangedAsync(_searchText);
            Items = newItems.ToList();

            await SetLoadingStateAsync(false);
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