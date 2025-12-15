using BlazorDrop.Interfaces;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Services
{
    internal class BlazorDropInputInteropService : IBlazorDropInputInteropService
    {
        private const string InitMethod = "BlazorDropSelect.initInputHandler";

        private readonly IJSRuntime _js;

        public BlazorDropInputInteropService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task RegisterAsync<T>(
            string inputElementId,
            int debounceDelayMs,
            string clickOutsideContainerId,
            DotNetObjectReference<T> dotNetRef)
            where T : class
        {
            await _js.InvokeVoidAsync(
                InitMethod,
                dotNetRef,
                inputElementId,
                debounceDelayMs,
                clickOutsideContainerId);
        }
    }
}
