using app.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace app.BackgroundWorker
{
    public class BackgroundCoin : BackgroundService
    {
        private readonly ICoinWorker _coinWorker;

        public BackgroundCoin(ICoinWorker coinWorker)
        {
            _coinWorker = coinWorker;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _coinWorker.DoWork(stoppingToken);
        }
    }
}
