using BlazorDrop.Data;
using BlazorDrop.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base.Select
{
    public abstract class BaseLazySelectableComponent<TItem> : BaseLazyComponent, IBlazorDropInvokable, IAsyncDisposable, IDisposable
    {

        [Parameter] public IEnumerable<TItem>? Items { get; set; }

        [Parameter] public IBlazorDropDataSource<TItem>? DataSource { get; set; }

        [Parameter] public Func<BlazorDropQuery, CancellationToken, Task<IEnumerable<TItem>>>? OnQueryAsync { get; set; }

        [Parameter] public Func<BlazorDropQuery, IEnumerable<TItem>>? OnQuery { get; set; }

        [Parameter] public Func<int, int, Task<IEnumerable<TItem>>>? OnLoadItemsAsync { get; set; }

        [Parameter] public Func<int, int, IEnumerable<TItem>>? OnLoadItems { get; set; }

        [Parameter] public int PageSize { get; set; } = 20;

        [Parameter] public int CurrentPage { get; set; }

        [Parameter] public Func<TItem, string>? DisplaySelector { get; set; }

        [Parameter] public Func<TItem, object?>? KeySelector { get; set; }

        [Parameter] public IEqualityComparer<TItem>? ItemComparer { get; set; }

        [Parameter] public StringComparison SearchComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

        [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }

        [Parameter] public RenderFragment? EmptyTemplate { get; set; }

        [Parameter] public RenderFragment? LoadingTemplate { get; set; }

        [Parameter] public RenderFragment? NoMoreDataTemplate { get; set; }

        [Parameter] public string? ValueNotFoundMessageText { get; set; }

        [Parameter] public bool ShowLoadingIndicator { get; set; } = true;

        [Parameter] public bool Dense { get; set; }

        [Parameter] public bool FullWidth { get; set; }

        [Parameter] public string? MaxDropdownHeight { get; set; }

        [Parameter] public BlazorDropTexts? Texts { get; set; }

        [Parameter] public Func<TItem, Task<TItem>>? OnItemClickAsync { get; set; }

        [Parameter] public EventCallback OnReachedEnd { get; set; }

        [Parameter] public EventCallback<Exception> OnError { get; set; }

        [CascadingParameter] private EditContext? CascadedEditContext { get; set; }

        [CascadingParameter] private BlazorDropTexts? CascadedTexts { get; set; }

        [Inject] internal IBlazorDropInteropService InteropService { get; set; } = default!;

        private readonly List<TItem> _items = new List<TItem>();
        private int _lastPage;
        private int _version;
        private CancellationTokenSource? _cts;
        private string _appliedSearch = string.Empty;
        private BrowseSnapshot? _snapshot;
        private IEnumerable<TItem>? _boundItems;
        private bool _initialized;
        private bool _disposed;
        private bool _itemsChanged;
        private DotNetObjectReference<IBlazorDropInvokable>? _dotNetRef;
        private FieldIdentifier _field;
        private bool _hasField;

        protected internal IReadOnlyList<TItem> RenderItems => _items;

        protected internal bool IsLoading { get; private set; }

        protected internal bool HasMore { get; private set; } = true;

        protected internal Exception? LoadError { get; private set; }

        protected internal int ActiveIndex { get; private set; } = -1;

        protected bool IsScrollAttached { get; private set; }

        protected bool IsDisposed => _disposed;

        protected internal BlazorDropTexts Strings => Texts ?? CascadedTexts ?? BlazorDropTexts.Default;

        protected internal string ListboxId => Id + "-listbox";

        protected internal string GetOptionId(int index) => Id + "-opt-" + index;

        protected internal string? ActiveDescendantId
            => ActiveIndex >= 0 && ActiveIndex < _items.Count ? GetOptionId(ActiveIndex) : null;

        protected internal string EmptyText => ValueNotFoundMessageText ?? Strings.NoItems;

        protected internal string FieldCssClass
            => _hasField && CascadedEditContext != null ? CascadedEditContext.FieldCssClass(_field) : string.Empty;

        protected internal virtual string RootCssClass
            => Css("bzd-container", FullWidth ? "bzd-full-width" : null, Dense ? "bzd-dense" : null, FieldCssClass, Class);

        protected internal string? MaxHeightStyle
            => string.IsNullOrEmpty(MaxDropdownHeight) ? null : "max-height:" + MaxDropdownHeight + ";";

        private protected DotNetObjectReference<IBlazorDropInvokable>? DotNetRef => _dotNetRef;

        protected abstract string ScrollContainerId { get; }

        protected abstract string FocusTargetId { get; }

        protected virtual string QuerySearchText => string.Empty;

        protected override async Task OnInitializedAsync()
        {
            _dotNetRef = DotNetObjectReference.Create<IBlazorDropInvokable>(this);
            _boundItems = Items;
            await OnInitializedCoreAsync();
            await ReloadCoreAsync();
            _initialized = true;
        }

        protected virtual Task OnInitializedCoreAsync() => Task.CompletedTask;

        protected override async Task OnParametersSetAsync()
        {
            if (_initialized && !ReferenceEquals(Items, _boundItems))
            {
                _boundItems = Items;
                await ReloadCoreAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_itemsChanged)
            {
                _itemsChanged = false;
                if (HasMore && IsScrollAttached)
                {
                    await SafeInteropAsync(() => InteropService.CheckScrollEndAsync(ScrollContainerId));
                }
            }
        }

        public async Task ReloadAsync()
        {
            await ReloadCoreAsync();
            await InvokeAsync(StateHasChanged);
        }

        public async Task SelectItemAsync(TItem item)
        {
            var value = item;
            if (OnItemClickAsync != null)
            {
                await SetLoadingAsync(true);
                try
                {
                    value = await OnItemClickAsync(item);
                }
                finally
                {
                    await SetLoadingAsync(false);
                }
            }

            await OnItemResolvedAsync(value);
            await InvokeAsync(StateHasChanged);
        }

        public Task FocusAsync() => SafeInteropAsync(() => InteropService.FocusAsync(FocusTargetId));

        [JSInvokable]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task OnScrollToEndAsync() => LoadNextPageAsync();

        protected abstract Task OnItemResolvedAsync(TItem item);

        protected internal abstract bool IsItemSelected(TItem item);

        protected bool ItemEquals(TItem a, TItem b)
        {
            if (a is null || b is null)
            {
                return a is null && b is null;
            }

            if (KeySelector != null)
            {
                return Equals(KeySelector(a), KeySelector(b));
            }

            return (ItemComparer ?? EqualityComparer<TItem>.Default).Equals(a, b);
        }

        protected internal string GetDisplayValue(TItem item)
            => DisplaySelector?.Invoke(item) ?? item?.ToString() ?? string.Empty;

        protected internal string GetItemCssClass(TItem item, int index)
            => Css("bzd-item", IsItemSelected(item) ? "bzd-item-selected" : null, index == ActiveIndex ? "bzd-item-active" : null);

        protected static string Css(params string?[] parts)
            => string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));

        private protected virtual Func<string, CancellationToken, Task<IEnumerable<TItem>>>? GetSearchDelegate() => null;

        private protected DataSourceBinding<TItem> ResolveSource()
        {
            if (DataSource != null)
            {
                return new DataSourceBinding<TItem>(DataSource, true);
            }

            if (OnQueryAsync != null)
            {
                return new DataSourceBinding<TItem>(BlazorDropDataSource.FromDelegate(OnQueryAsync), true);
            }

            if (OnQuery != null)
            {
                return new DataSourceBinding<TItem>(BlazorDropDataSource.FromDelegate(OnQuery), true);
            }

            if (Items != null)
            {
                return new DataSourceBinding<TItem>(BlazorDropDataSource.FromEnumerable(Items, GetDisplayValue, SearchComparison), true);
            }

            var loader = GetPageLoader();
            if (loader != null)
            {
                var search = GetSearchDelegate();
                return new DataSourceBinding<TItem>(BlazorDropDataSource.FromPagedLoader(loader, search), search != null);
            }

            return new DataSourceBinding<TItem>(BlazorDropDataSource.Empty<TItem>(), true);
        }

        private Func<int, int, CancellationToken, Task<IEnumerable<TItem>>>? GetPageLoader()
        {
            if (OnLoadItemsAsync != null)
            {
                var load = OnLoadItemsAsync;
                return (page, size, ct) => load(page, size);
            }

            if (OnLoadItems != null)
            {
                var load = OnLoadItems;
                return (page, size, ct) => Task.FromResult(load(page, size));
            }

            return null;
        }

        private async Task ReloadCoreAsync()
        {
            _snapshot = null;
            var text = QuerySearchText;
            if (text.Length == 0 || ResolveSource().SupportsSearch)
            {
                _appliedSearch = text;
                await FetchAsync(text.Length == 0 ? CurrentPage : 0, reset: true);
                return;
            }

            _appliedSearch = string.Empty;
            await FetchAsync(CurrentPage, reset: true);
            await ApplySearchAsync();
        }

        protected async Task ApplySearchAsync()
        {
            var text = QuerySearchText;
            if (text == _appliedSearch)
            {
                return;
            }

            if (ResolveSource().SupportsSearch)
            {
                _snapshot = null;
                _appliedSearch = text;
                await FetchAsync(text.Length == 0 ? CurrentPage : 0, reset: true);
                return;
            }

            if (text.Length == 0)
            {
                RestoreSnapshot();
                return;
            }

            if (_snapshot == null)
            {
                if (IsLoading || LoadError != null)
                {
                    _appliedSearch = string.Empty;
                    if (!await FetchAsync(CurrentPage, reset: true))
                    {
                        return;
                    }
                }

                var copy = new List<TItem>(_items);
                _snapshot = new BrowseSnapshot(copy, _lastPage, HasMore, BlazorDropDataSource.FromEnumerable(copy, GetDisplayValue, SearchComparison));
            }

            _appliedSearch = text;
            await FetchAsync(0, reset: true);
        }

        private void RestoreSnapshot()
        {
            _version++;
            CancelPending();
            _items.Clear();
            if (_snapshot != null)
            {
                _items.AddRange(_snapshot.Items);
                _lastPage = _snapshot.LastPage;
                HasMore = _snapshot.HasMore;
                _snapshot = null;
            }

            _appliedSearch = string.Empty;
            LoadError = null;
            IsLoading = false;
            ActiveIndex = -1;
            _itemsChanged = true;
        }

        protected async Task<bool> LoadNextPageAsync()
        {
            if (IsLoading || !HasMore)
            {
                return false;
            }

            if (!await FetchAsync(_lastPage + 1, reset: false))
            {
                return false;
            }

            await OnReachedEnd.InvokeAsync(null);
            return true;
        }

        private async Task<bool> FetchAsync(int page, bool reset)
        {
            if (!reset && (IsLoading || !HasMore))
            {
                return false;
            }

            if (reset)
            {
                _version++;
                CancelPending();
                _items.Clear();
                HasMore = true;
                ActiveIndex = -1;
                _itemsChanged = true;
            }

            var version = _version;
            var source = _snapshot != null ? _snapshot.Source : ResolveSource().Source;
            var token = CurrentToken();
            LoadError = null;
            await SetLoadingAsync(true);
            try
            {
                var query = new BlazorDropQuery(page, Math.Max(1, PageSize), _appliedSearch);
                var result = await source.GetPageAsync(query, token);
                if (version != _version)
                {
                    return false;
                }

                if (result != null)
                {
                    _items.AddRange(result.Items);
                    HasMore = result.HasMore;
                }
                else
                {
                    HasMore = false;
                }

                _lastPage = page;
                _itemsChanged = true;
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                if (version != _version)
                {
                    return false;
                }

                LoadError = ex;
                await OnError.InvokeAsync(ex);
                return false;
            }
            finally
            {
                if (version == _version)
                {
                    await SetLoadingAsync(false);
                }
            }
        }

        private CancellationToken CurrentToken()
        {
            if (_cts == null)
            {
                _cts = new CancellationTokenSource();
            }

            return _cts.Token;
        }

        private void CancelPending()
        {
            if (_cts == null)
            {
                return;
            }

            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            _cts.Dispose();
            _cts = null;
        }

        private async Task SetLoadingAsync(bool isLoading)
        {
            IsLoading = isLoading;
            await InvokeAsync(StateHasChanged);
        }

        protected internal async Task HandleNavigationKeyAsync(string key)
        {
            switch (key)
            {
                case "ArrowDown":
                    await MoveActiveAsync(1);
                    break;
                case "ArrowUp":
                    await MoveActiveAsync(-1);
                    break;
                case "Home":
                    await SetActiveAsync(0);
                    break;
                case "End":
                    await SetActiveAsync(_items.Count - 1);
                    break;
                case "Enter":
                    if (LoadError != null && _items.Count == 0)
                    {
                        await ReloadAsync();
                    }
                    else if (ActiveIndex >= 0 && ActiveIndex < _items.Count)
                    {
                        await SelectItemAsync(_items[ActiveIndex]);
                    }

                    break;
            }
        }

        protected internal void SetHoverIndex(int index) => ActiveIndex = index;

        protected void ClearActiveIndex() => ActiveIndex = -1;

        private async Task MoveActiveAsync(int delta)
        {
            if (_items.Count == 0)
            {
                return;
            }

            var next = ActiveIndex + delta;
            if (next >= _items.Count && HasMore && !IsLoading)
            {
                await LoadNextPageAsync();
            }

            await SetActiveAsync(next);
        }

        private async Task SetActiveAsync(int index)
        {
            if (_items.Count == 0)
            {
                return;
            }

            ActiveIndex = Math.Max(0, Math.Min(index, _items.Count - 1));
            await SafeInteropAsync(() => InteropService.ScrollIntoViewAsync(GetOptionId(ActiveIndex)));
        }

        protected void BindField<TField>(Expression<Func<TField>>? expression)
        {
            if (expression == null)
            {
                _hasField = false;
                return;
            }

            _field = FieldIdentifier.Create(expression);
            _hasField = true;
        }

        protected void NotifyFieldChanged()
        {
            if (_hasField)
            {
                CascadedEditContext?.NotifyFieldChanged(_field);
            }
        }

        protected async Task AttachScrollAsync()
        {
            if (IsScrollAttached || _dotNetRef == null)
            {
                return;
            }

            IsScrollAttached = true;
            await InteropService.RegisterScrollAsync(ScrollContainerId, _dotNetRef);
        }

        protected async Task DetachScrollAsync()
        {
            IsScrollAttached = false;
            await SafeInteropAsync(() => InteropService.UnregisterScrollAsync(ScrollContainerId));
        }

        protected virtual Task ReleaseInteropAsync() => DetachScrollAsync();

        protected static async Task SafeInteropAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
#if NET6_0_OR_GREATER
            catch (JSDisconnectedException)
            {
            }
#endif
            catch (JSException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            CancelPending();
            if (_dotNetRef != null)
            {
                await ReleaseInteropAsync();
                _dotNetRef.Dispose();
                _dotNetRef = null;
            }

            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            _ = DisposeAsync().AsTask();
            GC.SuppressFinalize(this);
        }

        private sealed class BrowseSnapshot
        {
            public BrowseSnapshot(List<TItem> items, int lastPage, bool hasMore, IBlazorDropDataSource<TItem> source)
            {
                Items = items;
                LastPage = lastPage;
                HasMore = hasMore;
                Source = source;
            }

            public List<TItem> Items { get; }

            public int LastPage { get; }

            public bool HasMore { get; }

            public IBlazorDropDataSource<TItem> Source { get; }
        }
    }
}
