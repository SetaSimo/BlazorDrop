using System;

namespace BlazorDrop.Data
{
    public sealed class BlazorDropQuery
    {
        public BlazorDropQuery(int page, int pageSize, string? searchText)
        {
            if (page < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            Page = page;
            PageSize = pageSize;
            SearchText = searchText ?? string.Empty;
        }

        public int Page { get; }

        public int PageSize { get; }

        public int Skip => Page * PageSize;

        public string SearchText { get; }

        public bool HasSearch => SearchText.Length > 0;
    }
}
