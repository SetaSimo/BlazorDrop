using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base
{
    public abstract class BaseLazyLoadWithSelect<T> : BaseLazyScrollingComponent<T>
    {
        [Parameter]
        public T Value { get; set; }

        [Parameter]
        public Func<T, Task<T>> OnItemClickAsync { get; set; }

        private const string DefaultSelectedItemClass = "bzd-item";

        protected string GetSelectableItemClass(T value) => IsItemSelected(value)
            ? $"{DefaultSelectedItemClass} bzd-item-selected" : DefaultSelectedItemClass;

        protected virtual async Task HandleItemSelectedAsync(T value)
        {
            if (OnItemClickAsync == null)
            {
                Value = value;
            }
            else
            {
                ShowLoadingProgress(true);
                Value = await OnItemClickAsync.Invoke(value);
                ShowLoadingProgress(false);
            }
        }

        protected bool IsItemSelected(T item)
        {
            return EqualityComparer<T>.Default.Equals(item, Value);
        }
    }
}
