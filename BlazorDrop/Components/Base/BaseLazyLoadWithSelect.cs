using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorDrop.Components.Base
{
    public class BaseLazyLoadWithSelect<T> : BaseLazyScrollingComponent<T>
    {
        [Parameter]
        public T Value { get; set; }

        protected string SelectableItemClass => string.IsNullOrEmpty(SelectedItemClass) ? DefaultSelectedItemClass : $"{DefaultSelectedItemClass} {SelectedItemClass}";

        private const string DefaultSelectedItemClass = "selected-item";

        protected bool IsItemSelected(T item)
        {
            return EqualityComparer<T>.Default.Equals(item, Value);
        }
    }
}
