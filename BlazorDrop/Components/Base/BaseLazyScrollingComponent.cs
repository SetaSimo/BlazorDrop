using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base
{
    public abstract class BaseLazyScrollingComponent<T> : BaseLazyComponent
    {
        [Parameter]
        public int PageSize { get; set; } = 20;

        [Parameter]
        public int CurrentPage { get; set; } = 0;

        [Parameter]
        public string ValueNotFoundMessageText { get; set; }

        [Parameter]
        public bool ShowLoadingIndicator { get; set; } = true;

        /// <summary>
        /// first parameter is page number, second parameter is page size
        /// </summary>
        [Parameter]
        public Func<int, int, Task<IEnumerable<T>>> OnLoadItemsAsync { get; set; }

        [Parameter]
        public Func<T, string> DisplaySelector { get; set; }

        [Parameter]
        public EventCallback<bool> OnLoadingStateChanged { get; set; }

        [Parameter]
        public List<T> Items { get; set; } = new List<T>();

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        protected bool _isLoading = false;
        protected bool _hasLoadedAllItems = false;
        protected bool _isScrollHandlerAttached = false;
        protected bool _isFirstLoad = true;

        [JSInvokable]
        public async Task OnScrollToEndAsync()
        {
            if (_isLoading)
            {
                return;
            }

            await LoadNextPageAsync();
            StateHasChanged();
        }

        protected async Task LoadNextPageAsync()
        {
            CurrentPage++;
            await LoadPageAsync(CurrentPage);
        }

        protected virtual async Task LoadPageAsync(int pageNumber)
        {
            if (OnLoadItemsAsync == null || _hasLoadedAllItems || (_isLoading && _isFirstLoad is false))
                return;

            await SetLoadingStateAsync(true);

            var newItems = await OnLoadItemsAsync(pageNumber, PageSize);
            Items.AddRange(newItems);

            _hasLoadedAllItems = newItems == null || newItems?.Count() == 0;
            await SetLoadingStateAsync(false);
        }

        protected async Task SetLoadingStateAsync(bool isLoading)
        {
            _isLoading = isLoading;

            await NotifyLoadingChangedAsync(isLoading);

            StateHasChanged();
        }

        private async Task NotifyLoadingChangedAsync(bool isLoading)
        {
            if (OnLoadingStateChanged.HasDelegate)
            {
                await OnLoadingStateChanged.InvokeAsync(isLoading);
            }
        }

        protected string GetDisplayValue(T item)
        {
            if (DisplaySelector == null)
            {
                return item.ToString();
            }

            return DisplaySelector(item);
        }

        protected async Task UnregisterScrollHandlerAsync(string scrollContainerId)
        {
            await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterScrollHandler", scrollContainerId);
        }

        protected async Task RegisterScrollHandler<R>(string id, string methodName, DotNetObjectReference<R> dotNerRef) where R : class
        {
            await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerScrollHandler", dotNerRef, id, methodName);
        }
    }
}
