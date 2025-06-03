using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base.Select
{
    public abstract class BaseLazyInputWithSelect<T> : BaseLazySelectableComponent<T>
    {
        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public int UpdateSearchDelayInMilliseconds { get; set; } = 1000;

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public Func<string, Task<IEnumerable<T>>> OnSearchTextChangedAsync { get; set; }

        protected string _searchText = string.Empty;

        protected bool _isDropdownOpen = false;

        protected string _inputSelectorId = Guid.NewGuid().ToString();
        protected string _scrollSelectorId = Guid.NewGuid().ToString();
        protected string _scrollContainerId = Guid.NewGuid().ToString();

        [JSInvokable]
        public async Task CloseDropdownAsync()
        {
            _isDropdownOpen = false;
            _isScrollHandlerAttached = false;
            await UnregisterScrollHandlerAsync(_scrollSelectorId);
            StateHasChanged();
        }

        protected async Task OpenDropdownAsync()
        {
            if (Disabled)
                return;

            _isDropdownOpen = true;
        }

        [JSInvokable]
        public async Task UpdateSearchListAfterInputAsync()
        {
            if (OnSearchTextChangedAsync == null)
            {
                throw new ArgumentException($"{nameof(OnSearchTextChangedAsync)} is null");
            }

            await SetLoadingStateAsync(true);
            _hasLoadedAllItems = false;

            if (_isDropdownOpen is false)
            {
                await OpenDropdownAsync();
            }


            if (string.IsNullOrWhiteSpace(_searchText))
            {
                await ResetSearchAsync();
            }
            else
            {
                await SearchWithFilterAsync();
            }

            await SetLoadingStateAsync(false);
        }

        private async Task ResetSearchAsync()
        {
            CurrentPage = 0;
            Items = new List<T>();
            await LoadPageAsync(CurrentPage, true);
            StateHasChanged();
        }

        private async Task SearchWithFilterAsync()
        {
            await SetLoadingStateAsync(true);
            await UnregisterScrollHandlerAsync(_scrollSelectorId);

            var newItems = await OnSearchTextChangedAsync(_searchText);
            Items = newItems.ToList();

            await SetLoadingStateAsync(false);
        }
    }
}
