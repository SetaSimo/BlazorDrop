using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
