using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace app.Services.Spire
{
    public class SpireWorker:ICardWorker
    {
        private static long UN()
        {
            TimeSpan uTimeSpan = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)uTimeSpan.TotalSeconds;
        }

        public async Task<SpirePaymentResponse> SendPaymentGeneral(uint Сумма, byte ТипОперации, ushort Валюта, string Ip, int Port)
        {
            using (var client = new UdpClient())
            {
                uint Nop = 1;
                client.Connect(Ip, Port);
                var un = BitConverter.GetBytes(UN());
                List<byte[]> message = new List<byte[]>();
                message.Add(BitConverter.GetBytes(Nop));
                message.Add(un);
                message.Add(new byte[1] { ТипОперации });
                message.Add(BitConverter.GetBytes(Сумма));
                message.Add(BitConverter.GetBytes(Валюта));
                message.Add(BitConverter.GetBytes(0));
                byte[] Long = message.SelectMany(a => a).ToArray();
                await client.SendAsync(Long, Long.Length);

                ++Nop;
                while (true)
                {
                    message = new List<byte[]>();
                    message.Add(BitConverter.GetBytes(Nop));
                    message.Add(un);
                    byte[] Short = message.SelectMany(a => a).ToArray();
                    await client.SendAsync(Short, Short.Length);
                    var ShortResponse = await client.ReceiveAsync();
                    if (ShortResponse.Buffer[4] == 0)
                    {
                        return new SpirePaymentResponse();
                    }
                    if (ShortResponse.Buffer[4] == 2)
                    {
                        if (ShortResponse.Buffer.Length >= 52)
                            return new SpirePaymentResponse()
                            {
                                UNBank = Encoding.ASCII.GetString(ShortResponse.Buffer.Where(a => a > 12 && a < 25).ToArray()),
                                UNOperation = Encoding.ASCII.GetString(ShortResponse.Buffer.Where(a => a > 5 && a < 14).ToArray()),
                                CardNumber = Encoding.ASCII.GetString(ShortResponse.Buffer.Where(a => a > 32 && a < 52).ToArray())
                            };
                        else if (ShortResponse.Buffer.Length >= 25)
                            return new SpirePaymentResponse()
                            {
                                UNBank = Encoding.ASCII.GetString(ShortResponse.Buffer.Where(a => a > 12 && a < 25).ToArray()),
                                UNOperation = Encoding.ASCII.GetString(ShortResponse.Buffer.Where(a => a > 5 && a < 14).ToArray())
                            };
                        else
                            return new SpirePaymentResponse();
                    }
                }
            }
        }
    }
}

#region Info
//message[12] = ТипОперации; //0 – оплата товаров и услуг, 1 – возврат средств, 4 – отмена операции
#endregion
