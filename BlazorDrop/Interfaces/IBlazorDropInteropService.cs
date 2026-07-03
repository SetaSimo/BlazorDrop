using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorDrop.Interfaces
{
    internal interface IBlazorDropInteropService
    {
        Task RegisterClickOutsideAsync(string containerId, DotNetObjectReference<IBlazorDropInvokable> dotNetRef);

        Task UnregisterClickOutsideAsync(string containerId);

        Task RegisterInputAsync(string inputId, int debounceDelay, DotNetObjectReference<IBlazorDropInvokable> dotNetRef);

        Task UnregisterInputAsync(string inputId);

        Task RegisterScrollAsync(string containerId, DotNetObjectReference<IBlazorDropInvokable> dotNetRef);

        Task UnregisterScrollAsync(string containerId);

        Task CheckScrollEndAsync(string containerId);

        Task RegisterDropdownPositionAsync(string anchorId, string dropdownId);

        Task UnregisterDropdownPositionAsync(string dropdownId);

        Task RegisterKeyboardGuardAsync(string elementId);

        Task UnregisterKeyboardGuardAsync(string elementId);

        Task ScrollIntoViewAsync(string elementId);

        Task FocusAsync(string elementId);
    }
}
