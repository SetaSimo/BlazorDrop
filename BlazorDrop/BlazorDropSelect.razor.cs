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
        public Guid Id { get; set; } = Guid.NewGuid();

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
        public T Value { get; set; }

        [Parameter]
        public EventCallback<T> ValueChanged { get; set; }

        [Parameter]
        public Func<T, string> DisplaySelector { get; set; }

        /// <summary>
        /// first parameter is page number, second parameter is page size
        /// </summary>
        [Parameter]
        public Func<int, int, Task<IEnumerable<T>>> LoadItemsPagedAsync { get; set; }

        [Parameter]
        public Func<string, Task<IEnumerable<T>>> SearchByInputTextAsync { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        private Guid _inputSelectorId = Guid.NewGuid();
        private Guid _scrollContainerId = Guid.NewGuid();

        private List<T> Items { get; set; } = new List<T>();

        private DotNetObjectReference<BlazorDropSelect<T>> _dotNetRef;

        private string _searchText = string.Empty;

        private bool _didLoadAllItems = false;
        private bool _isDropdownOpen = false;
        private bool _didAddedScrollEventHandler = false;

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
            await LoadNextPageAsync();
            UpdateSearchTextAfterSelect(Value);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);

                await JSRuntime.InvokeVoidAsync("initBlazorDropSelect", _dotNetRef, _inputSelectorId, UpdateSearchDelayInMilliseconds);
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerClickOutsideHandler", _dotNetRef, _inputSelectorId);
            }

            if (_isDropdownOpen && _didAddedScrollEventHandler is false)
            {
                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerScrollHandler", _dotNetRef, _scrollContainerId);
                _didAddedScrollEventHandler = true;
            }
        }

        [JSInvokable]
        public async Task UpdateSearchListAfterInputAsync()
        {
            if (SearchByInputTextAsync == null)
                return;

            if (_isDropdownOpen is false)
                await OpenDropdown();

            _didLoadAllItems = false;

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                CurrentPage = 0;
                await LoadNextPageAsync();
                return;
            }

            var newItems = await SearchByInputTextAsync(_searchText);
            Items = newItems.ToList();
            StateHasChanged();
        }

        [JSInvokable]
        public async Task OnScrollToEndAsync()
        {
            await LoadNextPageAsync();
            StateHasChanged();
        }

        [JSInvokable]
        public async Task CloseDropdown()
        {
            _isDropdownOpen = false;
            await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterScrollHandler", _scrollContainerId);
            _didAddedScrollEventHandler = false;

            StateHasChanged();
        }

        private async Task OpenDropdown()
        {
            _isDropdownOpen = true;
            //if (_isDropdownOpen && _didAddedScrollEventHandler is false)
            //{
            //    await InvokeAsync(StateHasChanged);
            //    await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerScrollHandler", _dotNetRef, _scrollContainerId);
            //    _didAddedScrollEventHandler = true;
            //}
        }

        private async Task OnValueChangedAsync(T value)
        {
            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(value);
            }
            else
            {
                Value = value;
            }

            UpdateSearchTextAfterSelect(value);
        }

        private async Task LoadNextPageAsync()
        {
            if (LoadItemsPagedAsync == null || _didLoadAllItems)
                return;

            var newItems = await LoadItemsPagedAsync(CurrentPage, PageSize);
            Items.AddRange(newItems);
            _didLoadAllItems = newItems == null || newItems?.Count() == 0;
            CurrentPage++;
        }

        private void UpdateSearchTextAfterSelect(T value)
        {
            if (value == null)
                return;

            _searchText = GetDisplayValue(value);
        }

        public void Dispose()
        {
        }

        public async ValueTask DisposeAsync()
        {
            await JSRuntime.InvokeVoidAsync("lazyLoadSelect.unregisterClickOutsideHandler", _inputSelectorId);
            await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterScrollHandler", _scrollContainerId);

            _dotNetRef?.Dispose();
        }
    }
}