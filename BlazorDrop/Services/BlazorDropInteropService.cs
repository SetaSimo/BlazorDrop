using BlazorDrop.Interfaces;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Services
{
	internal class BlazorDropInteropService : IBlazorDropInteropService
	{
		private readonly IJSRuntime _js;
		private const string JsPrefix = "BlazorDropSelect";

		public BlazorDropInteropService(IJSRuntime js)
		{
			_js = js;
		}

		public async Task RegisterClickOutsideAsync(string containerId, DotNetObjectReference<IBlazorDropInvokable> dotNetRef)
			=> await _js.InvokeVoidAsync($"{JsPrefix}.registerClickOutsideHandler", dotNetRef, containerId);

		public async Task UnregisterClickOutsideAsync(string containerId)
			=> await _js.InvokeVoidAsync($"{JsPrefix}.unregisterClickOutsideHandler", containerId);

		public async Task RegisterInputAsync(string inputId, int debounceDelay, string containerId, DotNetObjectReference<IBlazorDropInvokable> dotNetRef)
			=> await _js.InvokeVoidAsync($"{JsPrefix}.initInputHandler", dotNetRef, inputId, debounceDelay, containerId);

		public async Task RegisterScrollAsync(string containerId, string callbackMethod, DotNetObjectReference<IBlazorDropInvokable> dotNetRef)
			=> await _js.InvokeVoidAsync($"{JsPrefix}.registerScrollHandler", dotNetRef, containerId, callbackMethod);

		public async Task UnregisterScrollAsync(string containerId)
			=> await _js.InvokeVoidAsync($"{JsPrefix}.unregisterScrollHandler", containerId);
	}
}
