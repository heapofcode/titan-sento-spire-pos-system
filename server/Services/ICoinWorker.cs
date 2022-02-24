using System.Threading;
using System.Threading.Tasks;

namespace app.Services
{
    public interface ICoinWorker
    {
        Task DoWork(CancellationToken cancellationToken);
    }
}