using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropMultiSelect<T>
    {
        [Parameter] public IList<T> SelectedValues { get; set; } = new List<T>();

        [Parameter] public EventCallback<IList<T>> SelectedValuesChanged { get; set; }

        [Parameter] public Expression<Func<IList<T>>>? SelectedValuesExpression { get; set; }

        [Parameter] public int MaxDisplayedChips { get; set; } = 3;

        [Parameter] public int MaxSelectedItems { get; set; }

        [Parameter] public bool CloseOnSelect { get; set; }

        private readonly List<T> _selected = new List<T>();

        protected int SelectedCount => _selected.Count;

        protected IEnumerable<T> VisibleChips => _selected.Take(Math.Max(0, MaxDisplayedChips));

        protected int OverflowCount => Math.Max(0, _selected.Count - Math.Max(0, MaxDisplayedChips));

        protected internal override string? InputPlaceholder => _selected.Count == 0 ? Placeholder : null;

        protected override Task OnInitializedCoreAsync()
        {
            SyncFromParameter();
            return Task.CompletedTask;
        }

        protected override void OnParametersSet()
        {
            BindField(SelectedValuesExpression);
            SyncFromParameter();
        }

        private void SyncFromParameter()
        {
            var incoming = SelectedValues ?? (IList<T>)Array.Empty<T>();
            if (incoming.Count == _selected.Count && incoming.Zip(_selected, ItemEquals).All(same => same))
            {
                return;
            }

            _selected.Clear();
            _selected.AddRange(incoming);
        }

        protected override async Task OnItemResolvedAsync(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                _selected.RemoveAt(index);
            }
            else
            {
                if (MaxSelectedItems > 0 && _selected.Count >= MaxSelectedItems)
                {
                    return;
                }

                _selected.Add(item);
            }

            await NotifySelectionChangedAsync();
            if (CloseOnSelect)
            {
                await CloseDropdownAsync();
            }
        }

        protected async Task RemoveAsync(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                return;
            }

            _selected.RemoveAt(index);
            await NotifySelectionChangedAsync();
            await FocusAsync();
            await InvokeAsync(StateHasChanged);
        }

        protected override Task OnBackspaceOnEmptyAsync()
            => _selected.Count > 0 ? RemoveAsync(_selected[_selected.Count - 1]) : Task.CompletedTask;

        private async Task NotifySelectionChangedAsync()
        {
            var snapshot = new List<T>(_selected);
            SelectedValues = snapshot;
            await SelectedValuesChanged.InvokeAsync(snapshot);
            NotifyFieldChanged();
        }

        protected internal override bool IsItemSelected(T item) => IndexOf(item) >= 0;

        protected internal override bool HasClearableSelection => _selected.Count > 0;

        protected override Task OnSelectionClearedAsync()
        {
            _selected.Clear();
            return NotifySelectionChangedAsync();
        }

        private int IndexOf(T item) => _selected.FindIndex(v => ItemEquals(v, item));
    }
}
