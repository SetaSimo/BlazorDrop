using BlazorDrop.Data;

namespace BlazorDrop.Components.Base.Select
{
    internal sealed class DataSourceBinding<TItem>
    {
        public DataSourceBinding(IBlazorDropDataSource<TItem> source, bool supportsSearch)
        {
            Source = source;
            SupportsSearch = supportsSearch;
        }

        public IBlazorDropDataSource<TItem> Source { get; }

        public bool SupportsSearch { get; }
    }
}
