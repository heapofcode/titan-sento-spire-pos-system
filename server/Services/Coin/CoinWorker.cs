using app.Model;
using app.Model.DBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace app.Services
{
    public class CoinWorker : ICoinWorker
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CoinWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            SerialPort serialPort = new SerialPort("COM1", 9600);
            //using (var scope = _scopeFactory.CreateScope())
            //{
            //    var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //    var setting = await _context.Settings
            //        .OrderBy(a => a.Id)
            //        .Include(b=>b.CoinDevice)
            //        .FirstOrDefaultAsync();
                
            //    serialPort.PortName = "COM1";
            //    serialPort.BaudRate = 9600;
            //}
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceiveHandler);
            serialPort.Open();
        }

        private async void DataReceiveHandler(object sender, SerialDataReceivedEventArgs e)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                SerialPort sp = (SerialPort)sender;
                int byteReceive = Convert.ToInt32(sp.ReadExisting());
                if (byteReceive > 0) 
                {
                    var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var unit = await _context.CurrencyNominations
                        .Where(a => a.ByteReceive == byteReceive)
                        .SingleOrDefaultAsync();

                    if (_context.TransientPayments.Any(a => a.Value == unit.Value))
                    {
                        var getValue = await _context.TransientPayments.Where(a => a.Value == unit.Value).FirstOrDefaultAsync();
                        getValue.Quantity = getValue.Quantity + 1;
                        getValue.Amount = getValue.Quantity * getValue.Value;
                        _context.TransientPayments.Update(getValue);
                    }
                    else 
                    {
                        await _context.TransientPayments.AddAsync(new TransientPayment { Value = unit.Value, Quantity = 1, Amount = unit.Value });
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }

    }
}
