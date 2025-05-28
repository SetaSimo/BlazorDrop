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

        protected List<T> Items { get; set; } = new List<T>();

        protected Guid _scrollContainerId = Guid.NewGuid();

        protected bool _hasLoadedAllItems = false;
        protected bool _isScrollHandlerAttached = false;

        [JSInvokable]
        public async Task OnScrollToEndAsync()
        {
            if (_isLoading)
            {
                return;
            }

            ShowLoadingProgress(true);
            await LoadNextPageAsync();
            StateHasChanged();
        }

        protected async Task LoadNextPageAsync()
        {
            CurrentPage++;
            await LoadPageAsync(CurrentPage);
        }

        protected async Task LoadPageAsync(int pageNumber)
        {
            if (OnLoadItemsAsync == null || _hasLoadedAllItems)
                return;

            ShowLoadingProgress(true);

            var newItems = await OnLoadItemsAsync(pageNumber, PageSize);
            Items.AddRange(newItems);

            _hasLoadedAllItems = newItems == null || newItems?.Count() == 0;
            ShowLoadingProgress(false);
        }

        protected void ShowLoadingProgress(bool isLoading)
        {
            if (ShowLoadingIndicator)
            {
                _isLoading = isLoading;
                StateHasChanged();
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

        protected async Task UnregisterScrollHandlerAsync()
        {
            await JSRuntime.InvokeVoidAsync("BlazorDropSelect.unregisterScrollHandler", _scrollContainerId);
        }
    }
}
