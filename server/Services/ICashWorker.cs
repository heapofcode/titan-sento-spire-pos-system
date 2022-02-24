using app.Model.DBase;
using app.Model.DTOs.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace app.Services
{
    public interface ICashWorker
    {
        void Connection(string host, string login, string password, string port, string speed);
        Task<object> SendPaymentGeneral(Payment payment, List<TransientDraft> transientDraft, HeaderRequest headerRequest, int checkNumber = 0);
        Task<object> SendPayment(float value);
        Task<object> SendReport(int reportType);
        Task<object> GetState();
        Task<object> LoadTranslations();
    }
}
