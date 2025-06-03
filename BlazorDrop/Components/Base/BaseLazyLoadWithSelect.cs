using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base
{
    public abstract class BaseLazyLoadWithSelect<T> : BaseLazyScrollingComponent<T>
    {
        [Parameter]
        public T Value { get; set; }

        [Parameter]
        public Func<T, Task<T>> OnItemClickAsync { get; set; }

        private const string DefaultSelectableItemClass = "bzd-item";

        protected string GetSelectableItemClass(T value) => IsItemSelected(value)
            ? $"{DefaultSelectableItemClass} bzd-item-selected" : DefaultSelectableItemClass;

        protected virtual async Task HandleItemSelectedAsync(T value)
        {
            if (OnItemClickAsync == null)
            {
                Value = value;
            }
            else
            {
                await SetLoadingStateAsync(true);
                Value = await OnItemClickAsync.Invoke(value);
                await SetLoadingStateAsync(false);
            }
        }

        protected bool IsItemSelected(T item)
        {
            return item.Equals(Value);
        }
    }
}
