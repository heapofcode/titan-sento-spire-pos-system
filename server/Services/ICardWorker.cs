using app.Services.Spire;
using System.Threading.Tasks;

namespace app.Services
{
    public interface ICardWorker
    {
        Task<SpirePaymentResponse> SendPaymentGeneral(uint Сумма, byte ТипОперации, ushort Валюта, string Ip, int Port);
    }
}
