using BlazorDrop.Interfaces;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorDrop.Services
{
	internal class BlazorDropScrollInteropService : IBlazorDropScrollInteropService
	{
		private const string RegisterMethod = "BlazorDropSelect.registerScrollHandler";
		private const string UnregisterMethod = "BlazorDropSelect.unregisterScrollHandler";

		private readonly IJSRuntime _js;

		public BlazorDropScrollInteropService(IJSRuntime js)
		{
			_js = js;
		}

		public async Task RegisterAsync<T>(
			string containerId,
			string callbackMethod,
			DotNetObjectReference<T> dotNetRef)
			where T : class
		{
			await _js.InvokeVoidAsync(
				RegisterMethod,
				dotNetRef,
				containerId,
				callbackMethod);
		}

		public async Task UnregisterAsync(string containerId)
		{
			await _js.InvokeVoidAsync(UnregisterMethod, containerId);
		}
	}
}
