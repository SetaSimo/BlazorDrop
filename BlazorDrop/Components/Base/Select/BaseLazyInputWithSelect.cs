using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base.Select
{
    public abstract class BaseLazyInputWithSelect<T, R> : BaseLazySelectableComponent<T> where R : class
    {
        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public int UpdateSearchDelayInMilliseconds { get; set; } = 1000;

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public Func<string, Task<IEnumerable<T>>> OnSearchTextChangedAsync { get; set; }

        protected DotNetObjectReference<R> DotNetRef { get; private set; }

        protected string _searchText = string.Empty;

        protected bool _isDropdownOpen = false;
        protected bool _dotNetRefCreated = false;

        protected string _inputSelectorId = Guid.NewGuid().ToString();
        protected string _scrollSelectorId = Guid.NewGuid().ToString();
        protected string _scrollContainerId = Guid.NewGuid().ToString();

        private const string BaseMethodName = "BlazorDropSelect";
        private const string ClickHandlerMethodName = $"{BaseMethodName}.initInputHandler";
        private const string UnregisterClickOutsideMethodName = $"{BaseMethodName}.unregisterClickOutsideHandler";
        private const string RegisterClickOutsideHandlerMethodName = $"{BaseMethodName}.registerClickOutsideHandler";

        [JSInvokable]
        public async Task OnClickOutsideAsync(string selectorId)
        {
            _isDropdownOpen = false;
            _isScrollHandlerAttached = false;

            await UnregisterScrollHandlerAsync(selectorId);
            await UnregisterClickOutsideHandler(selectorId);

            StateHasChanged();
        }

        [JSInvokable]
        public async Task UpdateSearchListAfterInputAsync(string clickOutsideSelectorId)
        {
            if (OnSearchTextChangedAsync == null)
            {
                throw new ArgumentException($"{nameof(OnSearchTextChangedAsync)} is null");
            }

            await SetLoadingStateAsync(true);
            _hasLoadedAllItems = false;

            if (_isDropdownOpen is false)
            {
                await OpenDropdownAsync(clickOutsideSelectorId);
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

        protected async Task OpenDropdownAsync(string clickOutsideSelectorId)
        {
            if (Disabled)
                return;

            _isDropdownOpen = true;
            StateHasChanged();

            await RegisterClickOutsideHandlerAsync(clickOutsideSelectorId);

            if (_isDropdownOpen && _isScrollHandlerAttached is false && Disabled is false)
            {
                await RegisterScrollHandlerAsync(clickOutsideSelectorId, nameof(OnScrollToEndAsync), DotNetRef);
            }
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

        protected void CreateDotNetRef()
        {
            if (_dotNetRefCreated is false)
            {
                DotNetRef = DotNetObjectReference.Create(this as R);
                _dotNetRefCreated = true;
            }
        }

        protected async Task RegisterInputHandlerAsync(string inputHandlerSelectorId, string clickOutsideSelectorId)
        {
            await JSRuntime.InvokeVoidAsync(ClickHandlerMethodName, DotNetRef, inputHandlerSelectorId, UpdateSearchDelayInMilliseconds, clickOutsideSelectorId);
        }

        private async Task RegisterClickOutsideHandlerAsync(string clickOutsideSelectorId)
        {
            await JSRuntime.InvokeVoidAsync(RegisterClickOutsideHandlerMethodName, DotNetRef, clickOutsideSelectorId);
        }

        protected async Task UnregisterClickOutsideHandler(string id)
        {
            await JSRuntime.InvokeVoidAsync(UnregisterClickOutsideMethodName, id);
        }
    }
}
