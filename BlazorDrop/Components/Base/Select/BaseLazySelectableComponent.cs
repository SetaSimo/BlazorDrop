using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base.Select
{
    public abstract class BaseLazySelectableComponent<T> : BaseLazyComponent
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
        public IEnumerable<T> Items { get; set; } = new List<T>();

        [Parameter]
        public RenderFragment<T> ItemTemplate { get; set; }


        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public Func<T, Task<T>> OnItemClickAsync { get; set; }

        protected const string DefaultSelectableItemClass = "bzd-item";
        protected const string RegisterScrollHandlerMethodName = "BlazorDropSelect.registerScrollHandler";
        protected const string UnregisterScrollHandlerMethodName = "BlazorDropSelect.unregisterScrollHandler";

        protected bool _isLoading = false;
        protected bool _hasLoadedAllItems = false;
        protected bool _isScrollHandlerAttached = false;

        [JSInvokable]
        public async Task OnScrollToEndAsync()
        {
            if (_isLoading)
            {
                return;
            }

            await LoadNextPageAsync();
            await InvokeAsync(StateHasChanged);
        }

        protected async Task LoadNextPageAsync()
        {
            CurrentPage++;
            await LoadPageAsync(CurrentPage);
        }

        protected virtual async Task LoadPageAsync(int pageNumber, bool ignoreLoadingState = false)
        {
            if (OnLoadItemsAsync == null || _hasLoadedAllItems || (_isLoading && ignoreLoadingState is false))
                return;

            await SetLoadingStateAsync(true);

            var newItems = await OnLoadItemsAsync(pageNumber, PageSize);
            Items = Items.Concat(newItems);

            _hasLoadedAllItems = newItems == null || newItems?.Count() == 0;
            await SetLoadingStateAsync(false);
        }


        protected async Task SetLoadingStateAsync(bool isLoading)
        {
            _isLoading = isLoading;
            await InvokeAsync(StateHasChanged);
        }

        protected string GetDisplayValue(T item)
        {
            if (DisplaySelector == null)
            {
                return item.ToString();
            }

            return DisplaySelector(item);
        }

        protected async Task RegisterScrollHandlerAsync<R>(string id, string methodName, DotNetObjectReference<R> dotNerRef) where R : class
        {
            await JSRuntime.InvokeVoidAsync(RegisterScrollHandlerMethodName, dotNerRef, id, methodName);
            _isScrollHandlerAttached = true;
        }

        protected async Task UnregisterScrollHandlerAsync(string scrollContainerId)
        {
            await JSRuntime.InvokeVoidAsync(UnregisterScrollHandlerMethodName, scrollContainerId);
        }

        protected string GetSelectableItemClass(T item) =>
            IsItemSelected(item)
                ? $"{DefaultSelectableItemClass} bzd-item-selected"
                : DefaultSelectableItemClass;

        protected abstract Task HandleItemSelectedAsync(T item);

        protected abstract bool IsItemSelected(T item);
    }
}
