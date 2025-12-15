using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Interfaces
{
	public interface IBlazorDropClickOutsideService
	{
		Task RegisterAsync<T>(
			string containerId,
			DotNetObjectReference<T> dotNetRef)
			where T : class;

		Task UnregisterAsync(string containerId);
	}
}
