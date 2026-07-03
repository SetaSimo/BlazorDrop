using System.Threading;
using System.Threading.Tasks;

namespace BlazorDrop.Data
{
    public interface IBlazorDropDataSource<TItem>
    {
        Task<BlazorDropPage<TItem>> GetPageAsync(BlazorDropQuery query, CancellationToken cancellationToken);
    }
}
