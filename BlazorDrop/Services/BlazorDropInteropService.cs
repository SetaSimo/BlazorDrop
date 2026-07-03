using BlazorDrop.Interfaces;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Services
{
    internal sealed class BlazorDropInteropService : IBlazorDropInteropService
    {
        private const string JsPrefix = "BlazorDropSelect";

        private readonly IJSRuntime _js;

        public BlazorDropInteropService(IJSRuntime js)
        {
            _js = js;
        }

        public Task RegisterClickOutsideAsync(string containerId, DotNetObjectReference<IBlazorDropInvokable> dotNetRef)
            => Invoke("registerClickOutsideHandler", dotNetRef, containerId);

        public Task UnregisterClickOutsideAsync(string containerId)
            => Invoke("unregisterClickOutsideHandler", containerId);

        public Task RegisterInputAsync(string inputId, int debounceDelay, DotNetObjectReference<IBlazorDropInvokable> dotNetRef)
            => Invoke("initInputHandler", dotNetRef, inputId, debounceDelay);

        public Task UnregisterInputAsync(string inputId)
            => Invoke("unregisterInputHandler", inputId);

        public Task RegisterScrollAsync(string containerId, DotNetObjectReference<IBlazorDropInvokable> dotNetRef)
            => Invoke("registerScrollHandler", dotNetRef, containerId);

        public Task UnregisterScrollAsync(string containerId)
            => Invoke("unregisterScrollHandler", containerId);

        public Task CheckScrollEndAsync(string containerId)
            => Invoke("checkScrollEnd", containerId);

        public Task RegisterDropdownPositionAsync(string anchorId, string dropdownId)
            => Invoke("registerDropdownPosition", anchorId, dropdownId);

        public Task UnregisterDropdownPositionAsync(string dropdownId)
            => Invoke("unregisterDropdownPosition", dropdownId);

        public Task RegisterKeyboardGuardAsync(string elementId)
            => Invoke("registerKeyboardGuard", elementId);

        public Task UnregisterKeyboardGuardAsync(string elementId)
            => Invoke("unregisterKeyboardGuard", elementId);

        public Task ScrollIntoViewAsync(string elementId)
            => Invoke("scrollIntoView", elementId);

        public Task FocusAsync(string elementId)
            => Invoke("focusElement", elementId);

        private Task Invoke(string method, params object[] args)
            => _js.InvokeVoidAsync(JsPrefix + "." + method, args).AsTask();
    }
}
