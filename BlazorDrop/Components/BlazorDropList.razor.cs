﻿using BlazorDrop.Components.Base.Select;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropList<T> : BaseLazySelectableComponent<T>, IAsyncDisposable
    {
        [Parameter]
        public T Value { get; set; }

        private DotNetObjectReference<BlazorDropList<T>> _dotNetRef;

        private bool _didLoadPageAfterInitialization = false;
        private bool _didAddScrollEvent = false;

        protected override async Task OnInitializedAsync()
        {
            if (Items.Any() is false)
            {
                await LoadPageAsync(CurrentPage);
            }

            _didLoadPageAfterInitialization = true;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_didLoadPageAfterInitialization && _didAddScrollEvent is false)
            {
                _didAddScrollEvent = true;
                _dotNetRef = DotNetObjectReference.Create(this);
                await RegisterScrollHandler(Id, nameof(OnScrollToEndAsync), _dotNetRef);
            }
        }

        protected override async Task HandleItemSelectedAsync(T value)
        {
            await SetLoadingStateAsync(true);

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

            await SetLoadingStateAsync(false);
        }

        protected override bool IsItemSelected(T item)
        {
            return EqualityComparer<T>.Default.Equals(item, Value);
        }

        public async ValueTask DisposeAsync()
        {
            if (_dotNetRef != null)
            {
                await UnregisterScrollHandlerAsync(Id);

                _dotNetRef.Dispose();
            }
        }
    }
}