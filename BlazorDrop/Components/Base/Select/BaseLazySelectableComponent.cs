using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorDrop.Interfaces;

namespace BlazorDrop.Components.Base.Select
{
    public abstract class BaseLazySelectableComponent<T, R> : BaseLazyComponent
        where R : class

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
        /// pageNumber, pageSize
        /// </summary>
        [Parameter]
        public Func<int, int, Task<IEnumerable<T>>> OnLoadItemsAsync { get; set; }

        [Parameter]
        public Func<T, string> DisplaySelector { get; set; }

        [Parameter]
        public IEnumerable<T> Items { get; set; } = new List<T>();

        [Parameter]
        public RenderFragment<T> ItemTemplate { get; set; }

        [Parameter]
        public Func<T, Task<T>> OnItemClickAsync { get; set; }

        [Inject]
        protected IBlazorDropScrollInteropService ScrollInterop { get; set; }

        protected DotNetObjectReference<R> DotNetRef { get; private set; }

        protected const string DefaultSelectableItemClass = "bzd-item";

        protected bool _isLoading;
        protected bool _hasLoadedAllItems;
        protected bool _isScrollHandlerAttached;
        protected bool _dotNetRefCreated;

        // ===== JS CALLBACK =====
        [JSInvokable]
        public async Task OnScrollToEndAsync()
        {
            if (_isLoading || _hasLoadedAllItems)
            {
                return;
            }

            await LoadNextPageAsync();
            await InvokeAsync(StateHasChanged);
        }

        protected async Task LoadNextPageAsync()
        {
            var nextPage = CurrentPage + 1;
            await LoadPageAsync(nextPage);
            CurrentPage = nextPage;
        }

        protected virtual async Task LoadPageAsync(int pageNumber, bool ignoreLoadingState = false)
        {
            if (OnLoadItemsAsync == null ||
                _hasLoadedAllItems ||
                (_isLoading && !ignoreLoadingState))
            {
                return;
            }

            await SetLoadingStateAsync(true);

            var newItems = (await OnLoadItemsAsync(pageNumber, PageSize))?.ToList()
                           ?? new List<T>();

            Items = Items.Concat(newItems);

            _hasLoadedAllItems = newItems.Count < PageSize;

            await SetLoadingStateAsync(false);
        }

        protected async Task SetLoadingStateAsync(bool isLoading)
        {
            _isLoading = isLoading;
            await InvokeAsync(StateHasChanged);
        }

        protected string GetDisplayValue(T item)
            => DisplaySelector?.Invoke(item) ?? item?.ToString();

        protected string GetSelectableItemClass(T item)
            => IsItemSelected(item)
                ? $"{DefaultSelectableItemClass} bzd-item-selected"
                : DefaultSelectableItemClass;

        protected async Task RegisterScrollAsync<R>(
            string containerId,
            DotNetObjectReference<R> dotNetRef)
            where R : class
        {
            if (_isScrollHandlerAttached)
                return;

            await ScrollInterop.RegisterAsync(
                containerId,
                nameof(OnScrollToEndAsync),
                dotNetRef);

            _isScrollHandlerAttached = true;
        }

        protected async Task UnregisterScrollAsync(string containerId)
        {
            if (!_isScrollHandlerAttached)
                return;

            await ScrollInterop.UnregisterAsync(containerId);
            _isScrollHandlerAttached = false;
        }

        protected void CreateDotNetRef()
        {
            if (_dotNetRefCreated)
            {
                return;
            }

            DotNetRef = DotNetObjectReference.Create((R)(object)this);
            _dotNetRefCreated = true;
        }

        protected abstract Task HandleItemSelectedAsync(T item);
        protected abstract bool IsItemSelected(T item);
    }
}
