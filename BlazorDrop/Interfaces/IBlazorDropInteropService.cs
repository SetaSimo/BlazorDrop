using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Interfaces
{
	public interface IBlazorDropInteropService
	{
		Task RegisterClickOutsideAsync(string containerId, DotNetObjectReference<IBlazorDropInvokable> dotNetRef);

		Task UnregisterClickOutsideAsync(string containerId);

		Task RegisterInputAsync(string inputId, int debounceDelay, string containerId, DotNetObjectReference<IBlazorDropInvokable> dotNetRef);

		Task RegisterScrollAsync(string containerId, string callbackMethod, DotNetObjectReference<IBlazorDropInvokable> dotNetRef);

		Task UnregisterScrollAsync(string containerId);
	}
}
