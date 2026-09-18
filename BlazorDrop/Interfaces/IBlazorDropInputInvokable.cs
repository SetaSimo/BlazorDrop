using System.Threading.Tasks;

namespace BlazorDrop.Interfaces
{
    internal interface IBlazorDropInputInvokable : IBlazorDropInvokable
    {
        Task OnClickOutsideAsync();

        Task UpdateSearchListAfterInputAsync();
    }
}
