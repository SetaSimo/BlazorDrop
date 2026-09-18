using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropSelect<T>
    {
        [Parameter] public T Value { get; set; } = default!;

        [Parameter] public EventCallback<T> ValueChanged { get; set; }

        [Parameter] public Expression<Func<T>>? ValueExpression { get; set; }

        private T _lastValue = default!;

        protected override Task OnInitializedCoreAsync()
        {
            _lastValue = Value;
            SyncSearchTextToValue();
            return Task.CompletedTask;
        }

        protected override void OnParametersSet()
        {
            BindField(ValueExpression);
            if (!ItemEquals(Value, _lastValue))
            {
                _lastValue = Value;
                if (!IsDropdownOpen)
                {
                    SyncSearchTextToValue();
                }
            }
        }

        private void SyncSearchTextToValue() => SearchText = Value is null ? string.Empty : GetDisplayValue(Value);

        protected override void ResetSearchTextOnClose() => SyncSearchTextToValue();

        protected override async Task OnItemResolvedAsync(T item)
        {
            Value = item;
            _lastValue = item;
            SyncSearchTextToValue();
            await ValueChanged.InvokeAsync(Value);
            NotifyFieldChanged();
            await CloseDropdownAsync();
        }

        protected internal override bool IsItemSelected(T item) => !(Value is null) && ItemEquals(item, Value);

        protected internal override bool HasClearableSelection => !(Value is null);

        protected override async Task OnSelectionClearedAsync()
        {
            Value = default!;
            _lastValue = default!;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
