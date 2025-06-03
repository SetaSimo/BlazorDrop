using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropList<T> : IAsyncDisposable
    {
        private DotNetObjectReference<BlazorDropList<T>> _dotNetRef;

        private bool _didLoadPageAfterInitialization = false;
        private bool _didAddScrollEvent = false;

        protected override async Task OnInitializedAsync()
        {
            if (Items.Any() is false)
            {
                await SetLoadingStateAsync(true);
                await LoadPageAsync(CurrentPage);
                await SetLoadingStateAsync(false);
            }

            _didLoadPageAfterInitialization = true;
            _isFirstLoad = false;
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