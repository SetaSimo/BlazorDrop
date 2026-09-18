using BlazorDrop.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base.Select
{
    public abstract class BaseLazyInputWithSelect<TItem> : BaseLazySelectableComponent<TItem>, IBlazorDropInputInvokable
    {
        [Parameter] public string? Placeholder { get; set; }

        [Parameter] public string? Label { get; set; }

        [Parameter] public int UpdateSearchDelayInMilliseconds { get; set; } = 500;

        [Parameter] public int MinSearchLength { get; set; }

        [Parameter] public bool Disabled { get; set; }

        [Parameter] public bool Clearable { get; set; }

        [Parameter] public DropdownVariant Variant { get; set; } = DropdownVariant.Filled;

        [Parameter] public Func<string, Task<IEnumerable<TItem>>>? SearchAsync { get; set; }

        [Parameter] public Func<string, IEnumerable<TItem>>? Search { get; set; }

        [Parameter] public EventCallback OnOpen { get; set; }

        [Parameter] public EventCallback OnClose { get; set; }

        [Parameter] public EventCallback OnCleared { get; set; }

        [Parameter] public EventCallback<string> OnSearchChanged { get; set; }

        private bool _inputRegistered;
        private bool _dropdownInteropAttached;
        private bool _isInteractive;
        private bool _searchDirty;
        private bool _inputPending;
        private string _searchText = string.Empty;

        protected internal string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? string.Empty;
                _inputPending = true;
            }
        }

        protected internal bool IsDropdownOpen { get; private set; }

        protected internal string InputId => Id + "-input";

        protected internal string DropdownElementId => Id + "-dropdown";

        protected internal virtual string? InputPlaceholder => Placeholder;

        protected internal abstract bool HasClearableSelection { get; }

        protected override string ScrollContainerId => DropdownElementId;

        protected override string FocusTargetId => InputId;

        protected override string QuerySearchText
        {
            get
            {
                if (!_searchDirty)
                {
                    return string.Empty;
                }

                var text = SearchText.Trim();
                return text.Length >= Math.Max(0, MinSearchLength) ? text : string.Empty;
            }
        }

        protected internal override string RootCssClass
            => Css(base.RootCssClass,
                   Variant == DropdownVariant.Outlined ? "bzd-outlined" : "bzd-filled",
                   Disabled ? "bzd-disabled" : null,
                   IsDropdownOpen ? "bzd-open" : null);

        private protected override Func<string, CancellationToken, Task<IEnumerable<TItem>>>? GetSearchDelegate()
        {
            if (SearchAsync != null)
            {
                var search = SearchAsync;
                return (text, ct) => search(text);
            }

            if (Search != null)
            {
                var search = Search;
                return (text, ct) => Task.FromResult(search(text));
            }

            return null;
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (Disabled && IsDropdownOpen)
            {
                await CloseDropdownAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _isInteractive = true;
            await SyncInputHandlerAsync(wanted: !Disabled);
            await SyncDropdownInteropAsync(wanted: IsDropdownOpen && !Disabled);
            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task ReleaseInteropAsync()
        {
            if (!_isInteractive)
            {
                return;
            }

            await SyncInputHandlerAsync(wanted: false);
            await SyncDropdownInteropAsync(wanted: false);
        }

        private async Task SyncInputHandlerAsync(bool wanted)
        {
            if (wanted == _inputRegistered || DotNetRef == null)
            {
                return;
            }

            _inputRegistered = wanted;
            if (wanted)
            {
                await InteropService.RegisterInputAsync(InputId, UpdateSearchDelayInMilliseconds, DotNetRef);
            }
            else
            {
                await SafeInteropAsync(() => InteropService.UnregisterInputAsync(InputId));
            }
        }

        private async Task SyncDropdownInteropAsync(bool wanted)
        {
            if (wanted == _dropdownInteropAttached || DotNetRef == null)
            {
                return;
            }

            _dropdownInteropAttached = wanted;
            if (!wanted)
            {
                await TeardownDropdownInteropAsync();
                return;
            }

            await InteropService.RegisterClickOutsideAsync(Id, DotNetRef);
            if (!DropdownInteropStillWanted)
            {
                await TeardownDropdownInteropAsync();
                return;
            }

            await AttachScrollAsync();
            if (!DropdownInteropStillWanted)
            {
                await TeardownDropdownInteropAsync();
                return;
            }

            await InteropService.RegisterDropdownPositionAsync(Id, DropdownElementId);
            if (!DropdownInteropStillWanted)
            {
                await TeardownDropdownInteropAsync();
            }
        }

        private bool DropdownInteropStillWanted => _dropdownInteropAttached && IsDropdownOpen && !IsDisposed;

        private async Task TeardownDropdownInteropAsync()
        {
            await SafeInteropAsync(() => InteropService.UnregisterClickOutsideAsync(Id));
            await DetachScrollAsync();
            await SafeInteropAsync(() => InteropService.UnregisterDropdownPositionAsync(DropdownElementId));
        }

        [JSInvokable]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public async Task OnClickOutsideAsync()
        {
            await CloseDropdownAsync();
            await InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public async Task UpdateSearchListAfterInputAsync()
        {
            if (Disabled || !_inputPending)
            {
                return;
            }

            _inputPending = false;
            _searchDirty = true;
            if (IsDropdownOpen)
            {
                await ApplySearchAsync();
            }
            else
            {
                await OpenDropdownAsync();
            }

            await OnSearchChanged.InvokeAsync(SearchText);
            await InvokeAsync(StateHasChanged);
        }

        public async Task OpenAsync()
        {
            await OpenDropdownAsync();
            await InvokeAsync(StateHasChanged);
        }

        public async Task CloseAsync()
        {
            await CloseDropdownAsync();
            await InvokeAsync(StateHasChanged);
        }

        public Task ClearAsync() => ClearSelectionAsync();

        protected internal async Task OpenDropdownAsync()
        {
            if (Disabled || IsDropdownOpen)
            {
                return;
            }

            IsDropdownOpen = true;
            ClearActiveIndex();
            await ApplySearchAsync();
            await OnOpen.InvokeAsync(null);
            await InvokeAsync(StateHasChanged);
        }

        protected internal async Task CloseDropdownAsync()
        {
            if (!IsDropdownOpen)
            {
                return;
            }

            IsDropdownOpen = false;
            ClearActiveIndex();
            _searchDirty = false;
            ResetSearchTextOnClose();
            _inputPending = false;
            await SyncDropdownInteropAsync(wanted: false);
            await ApplySearchAsync();
            await OnClose.InvokeAsync(null);
        }

        protected async Task ToggleDropdownAsync()
        {
            if (IsDropdownOpen)
            {
                await CloseDropdownAsync();
            }
            else
            {
                await OpenDropdownAsync();
            }
        }

        protected virtual void ResetSearchTextOnClose() => SearchText = string.Empty;

        protected internal async Task ClearSelectionAsync()
        {
            if (!HasClearableSelection)
            {
                return;
            }

            SearchText = string.Empty;
            _searchDirty = false;
            _inputPending = false;
            await OnSelectionClearedAsync();
            await ApplySearchAsync();
            NotifyFieldChanged();
            await OnCleared.InvokeAsync(null);
            await FocusAsync();
            await InvokeAsync(StateHasChanged);
        }

        protected abstract Task OnSelectionClearedAsync();

        protected internal async Task OnInputKeyDownAsync(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "Escape":
                    await CloseDropdownAsync();
                    return;
                case "Tab":
                    if (IsDropdownOpen)
                    {
                        await CloseDropdownAsync();
                    }

                    return;
                case "Backspace":
                    if (SearchText.Length == 0)
                    {
                        await OnBackspaceOnEmptyAsync();
                    }

                    return;
                case "ArrowDown":
                    if (!IsDropdownOpen)
                    {
                        await OpenDropdownAsync();
                    }

                    break;
                case "ArrowUp":
                case "Enter":
                    break;
                default:
                    return;
            }

            if (IsDropdownOpen)
            {
                await HandleNavigationKeyAsync(e.Key);
            }
        }

        protected virtual Task OnBackspaceOnEmptyAsync() => Task.CompletedTask;
    }
}
