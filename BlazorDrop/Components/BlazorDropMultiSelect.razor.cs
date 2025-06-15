using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropMultiSelect<T> : IAsyncDisposable
    {
        [Parameter]
        public Func<IEnumerable<T>, Task<string>> GetDisplayTextAsync { get; set; }

        public IList<T> SelectedValues { get; set; } = new List<T>();

        protected override async Task OnInitializedAsync()
        {
            CreateDotNetRef();
            await LoadPageAsync(CurrentPage);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Disabled is false)
            {
                await RegisterInputHandlerAsync(_inputSelectorId, Id);
            }

            if (_isDropdownOpen && _isScrollHandlerAttached is false && Disabled is false)
            {
                await RegisterScrollHandlerAsync(_scrollContainerId, nameof(OnScrollToEndAsync), DotNetRef);
            }
        }

        protected override async Task HandleItemSelectedAsync(T value)
        {
            await SetLoadingStateAsync(true);

            if (OnItemClickAsync != null)
            {
                value = await OnItemClickAsync.Invoke(value);
            }

            var wasItemAdded = SelectedValues.Any(v => EqualityComparer<T>.Default.Equals(v, value));

            if (wasItemAdded)
            {
                SelectedValues.Remove(value);
            }
            else
            {
                SelectedValues.Add(value);
            }

            if (GetDisplayTextAsync == null)
            {
                _searchText = string.Join(", ", SelectedValues.Select(x => GetDisplayValue(x)));
            }
            else
            {
                _searchText = await GetDisplayTextAsync(SelectedValues);
            }

            await SetLoadingStateAsync(false);
        }

        protected override bool IsItemSelected(T item)
        {
            return SelectedValues.Contains(item);
        }

        public async ValueTask DisposeAsync()
        {
            if (DotNetRef != null)
            {
                await UnregisterClickOutsideHandler(Id);
                await UnregisterScrollHandlerAsync(_scrollContainerId);

                DotNetRef.Dispose();
                _dotNetRefCreated = false;
            }
        }
    }
}