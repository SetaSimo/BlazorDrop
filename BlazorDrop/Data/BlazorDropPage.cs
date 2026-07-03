using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorDrop.Data
{
    public sealed class BlazorDropPage<TItem>
    {
        public static BlazorDropPage<TItem> Empty { get; } = new BlazorDropPage<TItem>(Array.Empty<TItem>(), false);

        public BlazorDropPage(IReadOnlyList<TItem> items, bool hasMore)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            HasMore = hasMore;
        }

        public IReadOnlyList<TItem> Items { get; }

        public bool HasMore { get; }
    }

    public static class BlazorDropPage
    {
        public static BlazorDropPage<TItem> Of<TItem>(IEnumerable<TItem>? items, bool hasMore)
            => new BlazorDropPage<TItem>(items == null ? new List<TItem>() : items.ToList(), hasMore);

        public static BlazorDropPage<TItem> FromItems<TItem>(IEnumerable<TItem>? items, int pageSize)
        {
            var list = items == null ? new List<TItem>() : items.ToList();
            return new BlazorDropPage<TItem>(list, list.Count >= pageSize);
        }

        public static BlazorDropPage<TItem> FromOverfetch<TItem>(List<TItem> items, int pageSize)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var hasMore = items.Count > pageSize;
            if (hasMore)
            {
                items.RemoveRange(pageSize, items.Count - pageSize);
            }

            return new BlazorDropPage<TItem>(items, hasMore);
        }
    }
}
