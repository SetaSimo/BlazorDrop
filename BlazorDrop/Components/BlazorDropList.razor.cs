using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
    public partial class BlazorDropList<T> : IAsyncDisposable, IDisposable
    {
        private DotNetObjectReference<BlazorDropList<T>> _dotNetRef;

        private bool _didLoadPageAfterInitialization = false;
        private bool _didAddScrollEvent = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadPageAsync(CurrentPage);
            _didLoadPageAfterInitialization = true;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_didLoadPageAfterInitialization && _didAddScrollEvent is false)
            {
                _didAddScrollEvent = true;
                _dotNetRef = DotNetObjectReference.Create(this);

                await JSRuntime.InvokeVoidAsync("BlazorDropSelect.registerScrollHandler", _dotNetRef, Id, nameof(OnScrollToEndAsync));
            }
        }

        public void Dispose()
        {
        }

        public async ValueTask DisposeAsync()
        {
            if (_dotNetRef != null)
            {
                await UnregisterScrollHandlerAsync();

                _dotNetRef.Dispose();
            }
        }
    }
}