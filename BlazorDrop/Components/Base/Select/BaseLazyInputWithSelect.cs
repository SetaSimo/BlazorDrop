using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorDrop.Interfaces;

namespace BlazorDrop.Components.Base.Select
{
    public abstract class BaseLazyInputWithSelect<T, R>
        : BaseLazySelectableComponent<T, R>
        where R : class
    {
        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public int UpdateSearchDelayInMilliseconds { get; set; } = 1000;

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public Func<string, Task<IEnumerable<T>>> SearchAsync { get; set; }

        [Inject]
        protected IBlazorDropClickOutsideService ClickOutsideService { get; set; }

        [Inject]
        protected IBlazorDropInputInteropService InputInterop { get; set; }

        protected string _searchText = string.Empty;

        protected bool _isDropdownOpen;

        protected readonly string _inputSelectorId = Guid.NewGuid().ToString();
        protected readonly string _scrollContainerId = Guid.NewGuid().ToString();

        [JSInvokable]
        public async Task OnClickOutsideAsync(string containerId)
        {
            _isDropdownOpen = false;
            _isScrollHandlerAttached = false;

            await UnregisterScrollAsync(containerId);
            await ClickOutsideService.UnregisterAsync(containerId);

            StateHasChanged();
        }

        [JSInvokable]
        public async Task UpdateSearchListAfterInputAsync(string containerId)
        {
            if (SearchAsync == null)
            {
                throw new InvalidOperationException($"{nameof(SearchAsync)} is null");
            }

            _hasLoadedAllItems = false;

            if (_isDropdownOpen is false)
            {
                await OpenDropdownAsync(containerId);
            }

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                await ResetSearchAsync();
            }
            else
            {
                await SearchWithFilterAsync();
            }
        }

        protected async Task OpenDropdownAsync(string containerId)
        {
            if (Disabled || _isDropdownOpen)
            {
                return;
            }

            _isDropdownOpen = true;
            StateHasChanged();

            await ClickOutsideService.RegisterAsync(containerId, DotNetRef);
            await RegisterScrollAsync(containerId, DotNetRef);
        }

        private async Task ResetSearchAsync()
        {
            CurrentPage = 0;
            Items = new List<T>();

            await LoadPageAsync(CurrentPage, ignoreLoadingState: true);
            StateHasChanged();
        }

        private async Task SearchWithFilterAsync()
        {
            await UnregisterScrollAsync(_scrollContainerId);

            var items = await SearchAsync(_searchText);
            Items = items?.ToList() ?? new List<T>();
        }

        protected async Task RegisterInputAsync(string clickOutsideContainerId)
        {
            if (Disabled)
            {
                return;
            }

            CreateDotNetRef();

            await InputInterop.RegisterAsync(
                _inputSelectorId,
                UpdateSearchDelayInMilliseconds,
                clickOutsideContainerId,
                DotNetRef);
        }
    }
}
