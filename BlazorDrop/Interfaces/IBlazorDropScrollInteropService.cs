using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Interfaces
{
	public interface IBlazorDropScrollInteropService
	{
		Task RegisterAsync<T>(
			string containerId,
			string callbackMethod,
			DotNetObjectReference<T> dotNetRef)
			where T : class;

		Task UnregisterAsync(string containerId);
	}

}
