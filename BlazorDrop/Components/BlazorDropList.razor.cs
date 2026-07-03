using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropList<T>
    {
        [Parameter] public T Value { get; set; } = default!;

        [Parameter] public EventCallback<T> ValueChanged { get; set; }

        [Parameter] public Expression<Func<T>>? ValueExpression { get; set; }

        private bool _keyboardGuardRegistered;

        protected override string ScrollContainerId => Id;

        protected override string FocusTargetId => ListboxId;

        protected internal override string RootCssClass => Css(base.RootCssClass, "bzd-list-container");

        protected string? ListStyle => string.Concat(MaxHeightStyle, Style);

        protected override void OnParametersSet() => BindField(ValueExpression);

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await AttachScrollAsync();
                _keyboardGuardRegistered = true;
                await InteropService.RegisterKeyboardGuardAsync(ListboxId);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task ReleaseInteropAsync()
        {
            await base.ReleaseInteropAsync();
            if (_keyboardGuardRegistered)
            {
                _keyboardGuardRegistered = false;
                await SafeInteropAsync(() => InteropService.UnregisterKeyboardGuardAsync(ListboxId));
            }
        }

        protected override async Task OnItemResolvedAsync(T item)
        {
            Value = item;
            await ValueChanged.InvokeAsync(Value);
            NotifyFieldChanged();
        }

        protected internal override bool IsItemSelected(T item) => !(Value is null) && ItemEquals(item, Value);
    }
}
