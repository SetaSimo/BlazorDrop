using BlazorDrop.Interfaces;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Services
{
	internal class BlazorDropClickOutsideService : IBlazorDropClickOutsideService
	{
		private const string RegisterMethod = "BlazorDropSelect.registerClickOutsideHandler";
		private const string UnregisterMethod = "BlazorDropSelect.unregisterClickOutsideHandler";

		private readonly IJSRuntime _js;

		public BlazorDropClickOutsideService(IJSRuntime js)
		{
			_js = js;
		}

		public async Task RegisterAsync<T>(
			string containerId,
			DotNetObjectReference<T> dotNetRef)
			where T : class
		{
			await _js.InvokeVoidAsync(RegisterMethod, dotNetRef, containerId);
		}

		public async Task UnregisterAsync(string containerId)
		{
			await _js.InvokeVoidAsync(UnregisterMethod, containerId);
		}
	}
}
