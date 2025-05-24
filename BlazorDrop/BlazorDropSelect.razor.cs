using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop
{
    public partial class BlazorDropSelect<T> : IAsyncDisposable, IDisposable
    {
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public string ValueNotFoundMessageText { get; set; }

        [Parameter]
        public int PageSize { get; set; } = 20;

        [Parameter]
        public int CurrentPage { get; set; } = 0;

        [Parameter]
        public int UpdateSearchDelayInMilliseconds { get; set; } = 1000;

        [Parameter]
        public bool CanShowLoadingIndicator { get; set; } = true;

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public T Value { get; set; }

        [Parameter]
        public Func<T, Task<T>> OnValueChangedAsync { get; set; }

        [Parameter]
        public Func<T, string> DisplaySelector { get; set; }

        /// <summary>
        /// first parameter is page number, second parameter is page size
        /// </summary>
        [Parameter]
        public Func<int, int, Task<IEnumerable<T>>> OnLoadItemsAsync { get; set; }

        [Parameter]
        public Func<string, Task<IEnumerable<T>>> OnSearchAsync { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        private List<T> Items { get; set; } = new List<T>();

        private DotNetObjectReference<BlazorDropSelect<T>> _dotNetRef;

        private Guid _inputSelectorId = Guid.NewGuid();
        private Guid _scrollContainerId = Guid.NewGuid();

        private string _searchText = string.Empty;

        private bool _hasLoadedAllItems = false;
        private bool _isDropdownOpen = false;
        private bool _isScrollHandlerAttached = false;
        private bool _isLoading = false;

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
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerScrollHandler", _dotNetRef, _scrollContainerId);
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
        public async Task OnScrollToEndAsync()
        {
            if (_isLoading)
            {
                return;
            }

            ShowLoadingIndicator(true);
            await LoadNextPageAsync();
            StateHasChanged();
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

        private async Task HandleItemSelectedAsync(T value)
        {
            if (Disabled)
                return;

            if (OnValueChangedAsync == null)
            {
                Value = value;
            }
            else
            {
                ShowLoadingIndicator(true);
                Value = await OnValueChangedAsync.Invoke(value);
                ShowLoadingIndicator(false);
            }

            UpdateSearchTextAfterSelect(Value);
        }

        private async Task LoadNextPageAsync()
        {
            CurrentPage++;
            await LoadPageAsync(CurrentPage);
        }

        private async Task LoadPageAsync(int pageNumber)
        {
            if (OnLoadItemsAsync == null || _hasLoadedAllItems)
                return;

            ShowLoadingIndicator(true);

            var newItems = await OnLoadItemsAsync(pageNumber, PageSize);
            Items.AddRange(newItems);

            _hasLoadedAllItems = newItems == null || newItems?.Count() == 0;
            ShowLoadingIndicator(false);
        }

        private void ShowLoadingIndicator(bool isLoading)
        {
            if (CanShowLoadingIndicator)
            {
                _isLoading = isLoading;
                StateHasChanged();
            }
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
            ShowLoadingIndicator(true);
            await UnregisterScrollHandlerAsync();

            var newItems = await OnSearchAsync(_searchText);
            Items = newItems.ToList();

            ShowLoadingIndicator(false);
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
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterClickOutsideHandler", _inputSelectorId);
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterScrollHandler", _scrollContainerId);

                _dotNetRef.Dispose();
            }
        }
    }
}