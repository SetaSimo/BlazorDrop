using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorDrop.Data
{
    public static class BlazorDropDataSource
    {
        public static IBlazorDropDataSource<TItem> FromEnumerable<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string>? displaySelector = null,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            => new EnumerableSource<TItem>(items ?? throw new ArgumentNullException(nameof(items)), displaySelector, comparison);

        public static IBlazorDropDataSource<TItem> FromDelegate<TItem>(Func<BlazorDropQuery, CancellationToken, Task<IEnumerable<TItem>>> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return new DelegateSource<TItem>(async (q, ct) => BlazorDropPage.FromItems(await query(q, ct), q.PageSize));
        }

        public static IBlazorDropDataSource<TItem> FromDelegate<TItem>(Func<BlazorDropQuery, CancellationToken, Task<BlazorDropPage<TItem>>> query)
            => new DelegateSource<TItem>(query ?? throw new ArgumentNullException(nameof(query)));

        public static IBlazorDropDataSource<TItem> FromDelegate<TItem>(Func<BlazorDropQuery, IEnumerable<TItem>> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return new DelegateSource<TItem>((q, ct) => Task.FromResult(BlazorDropPage.FromItems(query(q), q.PageSize)));
        }

        public static IBlazorDropDataSource<TItem> FromQueryable<TItem>(
            Func<BlazorDropQuery, IQueryable<TItem>> query,
            Func<IQueryable<TItem>, CancellationToken, Task<List<TItem>>>? materializeAsync = null)
            => new QueryableSource<TItem>(query ?? throw new ArgumentNullException(nameof(query)), materializeAsync);

        internal static IBlazorDropDataSource<TItem> FromPagedLoader<TItem>(
            Func<int, int, CancellationToken, Task<IEnumerable<TItem>>> loadPage,
            Func<string, CancellationToken, Task<IEnumerable<TItem>>>? search)
            => new PagedLoaderSource<TItem>(loadPage, search);

        internal static IBlazorDropDataSource<TItem> Empty<TItem>() => EmptySource<TItem>.Instance;

        private sealed class EnumerableSource<TItem> : IBlazorDropDataSource<TItem>
        {
            private readonly IEnumerable<TItem> _items;
            private readonly Func<TItem, string> _display;
            private readonly StringComparison _comparison;

            public EnumerableSource(IEnumerable<TItem> items, Func<TItem, string>? display, StringComparison comparison)
            {
                _items = items;
                _display = display ?? (x => x?.ToString() ?? string.Empty);
                _comparison = comparison;
            }

            public Task<BlazorDropPage<TItem>> GetPageAsync(BlazorDropQuery query, CancellationToken cancellationToken)
            {
                var source = _items;
                if (query.HasSearch)
                {
                    source = source.Where(x => (_display(x) ?? string.Empty).IndexOf(query.SearchText, _comparison) >= 0);
                }

                var list = source.Skip(query.Skip).Take(query.PageSize + 1).ToList();
                return Task.FromResult(BlazorDropPage.FromOverfetch(list, query.PageSize));
            }
        }

        private sealed class DelegateSource<TItem> : IBlazorDropDataSource<TItem>
        {
            private readonly Func<BlazorDropQuery, CancellationToken, Task<BlazorDropPage<TItem>>> _query;

            public DelegateSource(Func<BlazorDropQuery, CancellationToken, Task<BlazorDropPage<TItem>>> query) => _query = query;

            public Task<BlazorDropPage<TItem>> GetPageAsync(BlazorDropQuery query, CancellationToken cancellationToken)
                => _query(query, cancellationToken);
        }

        private sealed class QueryableSource<TItem> : IBlazorDropDataSource<TItem>
        {
            private readonly Func<BlazorDropQuery, IQueryable<TItem>> _query;
            private readonly Func<IQueryable<TItem>, CancellationToken, Task<List<TItem>>>? _materialize;

            public QueryableSource(Func<BlazorDropQuery, IQueryable<TItem>> query, Func<IQueryable<TItem>, CancellationToken, Task<List<TItem>>>? materialize)
            {
                _query = query;
                _materialize = materialize;
            }

            public async Task<BlazorDropPage<TItem>> GetPageAsync(BlazorDropQuery query, CancellationToken cancellationToken)
            {
                var page = _query(query).Skip(query.Skip).Take(query.PageSize + 1);
                var list = _materialize != null ? await _materialize(page, cancellationToken) : page.ToList();
                return BlazorDropPage.FromOverfetch(list, query.PageSize);
            }
        }

        private sealed class PagedLoaderSource<TItem> : IBlazorDropDataSource<TItem>
        {
            private readonly Func<int, int, CancellationToken, Task<IEnumerable<TItem>>> _load;
            private readonly Func<string, CancellationToken, Task<IEnumerable<TItem>>>? _search;

            public PagedLoaderSource(Func<int, int, CancellationToken, Task<IEnumerable<TItem>>> load, Func<string, CancellationToken, Task<IEnumerable<TItem>>>? search)
            {
                _load = load;
                _search = search;
            }

            public async Task<BlazorDropPage<TItem>> GetPageAsync(BlazorDropQuery query, CancellationToken cancellationToken)
            {
                if (query.HasSearch)
                {
                    if (_search == null || query.Page > 0)
                    {
                        return BlazorDropPage<TItem>.Empty;
                    }

                    return BlazorDropPage.Of(await _search(query.SearchText, cancellationToken), hasMore: false);
                }

                return BlazorDropPage.FromItems(await _load(query.Page, query.PageSize, cancellationToken), query.PageSize);
            }
        }

        private sealed class EmptySource<TItem> : IBlazorDropDataSource<TItem>
        {
            public static readonly EmptySource<TItem> Instance = new EmptySource<TItem>();

            public Task<BlazorDropPage<TItem>> GetPageAsync(BlazorDropQuery query, CancellationToken cancellationToken)
                => Task.FromResult(BlazorDropPage<TItem>.Empty);
        }
    }
}
