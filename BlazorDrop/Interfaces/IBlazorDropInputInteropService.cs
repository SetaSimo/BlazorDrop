using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Interfaces
{
    public interface IBlazorDropInputInteropService
    {
        Task RegisterAsync<T>(
            string inputElementId,
            int debounceDelayMs,
            string clickOutsideContainerId,
            DotNetObjectReference<T> dotNetRef)
            where T : class;
    }
}
