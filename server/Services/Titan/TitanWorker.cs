using app.Model.DBase;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using app.Model.DTOs.Requests;

namespace app.Services.Titan
{
    public class TitanWorker:ICashWorker
    {
        private static HttpClient client;

        #region//**************** Подумать над этим позже **************//
        //public void LoadDeviceData()
        //{
        //    PaymentTypes = (List<ExpandoObject>)((dynamic)_httpClient.RequestGet("/cgi/tbl/Pay", new Dictionary<string, string>(), false).Result);
        //    Departments = (List<ExpandoObject>)((dynamic)_httpClient.RequestGet("/cgi/tbl/Dep", new Dictionary<string, string>(), false).Result);
        //    Groups = (List<ExpandoObject>)((dynamic)_httpClient.RequestGet("/cgi/tbl/Grp", new Dictionary<string, string>(), false).Result);
        //    Taxes = (List<ExpandoObject>)((dynamic)_httpClient.RequestGet("/cgi/tbl/Tax", new Dictionary<string, string>(), false).Result);

        //    dynamic emptyObject = new ExpandoObject();
        //    emptyObject.id = 0;
        //    emptyObject.Prc = "Without taxes"; //TitanDictionary.GetTranslation("Without taxes");
        //    string alphabet = "АБВГДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ";
        //    int counter = 0;
        //    foreach (dynamic tax in Taxes)
        //    {
        //        char chr = alphabet[counter];
        //        tax.Prc = chr.ToString();
        //        counter++;
        //    }
        //    Taxes.Insert(0, emptyObject);
        //}
        #endregion

        #region//byte parsing
        public static byte[] Decompress(byte[] data)
        {
            byte[] array = new byte[256];
            List<byte[]> list = new List<byte[]>();
            int num = 0;
            MemoryStream memoryStream = new MemoryStream(data);
            memoryStream.ReadByte();
            memoryStream.ReadByte();
            DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
            int num2;
            while ((num2 = deflateStream.Read(array, 0, 256)) > 0)
            {
                if (num2 == 256)
                {
                    list.Add(array);
                    array = new byte[256];
                }
                else
                {
                    byte[] array2 = new byte[num2];
                    Array.Copy(array, 0, array2, 0, num2);
                    list.Add(array2);
                }
                num += num2;
            }
            byte[] array3 = new byte[num];
            num2 = 0;
            foreach (byte[] item in list)
            {
                Array.Copy(item, 0, array3, num2, item.Length);
                num2 += item.Length;
            }
            return array3;
        }
        #endregion

        #region//connection body
        public void Connection(string host, string login, string password, string port, string speed)
        {
            host = host.Trim().TrimEnd('/');
            CredentialCache credentialCache = new CredentialCache();
            credentialCache.Add(new Uri(host), "Digest", new NetworkCredential(login, password));
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.Credentials = credentialCache;
            httpClientHandler.PreAuthenticate = true;
            client = new HttpClient(httpClientHandler);
            client.BaseAddress = new Uri(host);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.ExpectContinue = false;
        }
        #endregion

        #region//get response
        public async Task<object> GetState()
        {
            return await get("/cgi/state", false);
        }
        public async Task<object> LoadTranslations() 
        {
            return await get("/desc", true);
        }
        public async Task<object> SendReport(int reportType)
        {
            return await get($"/cgi/proc/printreport?{reportType}", false);
        }
        #endregion

        #region//post response
        public async Task<object> SendPaymentGeneral(Payment payment, List<TransientDraft> transientDraft, HeaderRequest headerRequest, int checkNumber)
        {
            var generalPaymentsItems = new List<TitanGeneralPaymentsItem>();

            if (headerRequest.iscancel)
            {
                generalPaymentsItems.Add(new TitanGeneralPaymentsItem()
                {
                    VD = new VD { No = checkNumber.ToString() }
                });

                var data = new TitanGeneralPayments() { VD = generalPaymentsItems };
                return await post("/cgi/chk", JsonConvert.SerializeObject(data), false);
            }
            else
            {
                generalPaymentsItems.Add(new TitanGeneralPaymentsItem()
                {
                    C = new C() { Cm = "Кассир: Иванов Иван Иванович" }
                });

                foreach (var good in transientDraft)
                {
                    generalPaymentsItems.Add(new TitanGeneralPaymentsItem()
                    {
                        S = new S()
                        {
                            Code = Convert.ToUInt64(good.Good.VendoreCode),
                            Name = good.Good.GoodsName,
                            Dep = 1,
                            Grp = 1,
                            Price = (float)good.Good.Price,
                            Qty = (float)good.Quantity,
                            Tax = 0
                        }
                    });
                    if (good.DiscountPercent != 0)
                    {
                        generalPaymentsItems.Add(new TitanGeneralPaymentsItem() { D = new D() { Prc = -Convert.ToInt32(good.DiscountPercent) } });
                    }
                }

                if (payment.CashPayment > 0)
                    generalPaymentsItems.Add(new TitanGeneralPaymentsItem() { P = new P() { No = 1, Sum = (float)payment.CashPayment } });
                if (payment.CardPayment > 0)
                    generalPaymentsItems.Add(new TitanGeneralPaymentsItem() { P = new P() { No = 4, Sum = (float)payment.CardPayment } });

                object data;
                if(headerRequest.isreturn)
                    data = new TitanGeneralPayments() { R = generalPaymentsItems };
                else
                    data = new TitanGeneralPayments() { F = generalPaymentsItems };

                return await post("/cgi/chk", JsonConvert.SerializeObject(data), false);
            }

            //ЧекJson = "{\"VD\":[{\"VD\":{\"no\":\"" + ЧекАннулирования + "\"}}]}";
            
        }
        public async Task<object> SendPayment(float value) 
        {
            var paymentsItem = new List<TitanPaymentsItem>();
            paymentsItem.Add(new TitanPaymentsItem() {IO = new IO() { no = 1, sum = value } });
            var data = new TitanPayments() { IO = paymentsItem };
            return await post("/cgi/chk", JsonConvert.SerializeObject(data), false);
        }
        #endregion

        #region//waiter get and post
        public async Task<object> get(string url, bool isGzip)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);
                response.EnsureSuccessStatusCode();

                byte[] byteArray = Enumerable.ToArray(await response.Content.ReadAsByteArrayAsync());

                if (isGzip)
                {
                    byteArray = Decompress(byteArray);
                }
                string jsonRawData = (byteArray.Length < 3 || byteArray[0] != 239 || byteArray[1] != 187 || byteArray[2] != 191) ? Encoding.UTF8.GetString(byteArray) : Encoding.UTF8.GetString(byteArray, 3, byteArray.Length - 3);

                return jsonRawData;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<object> post(string url, string json, bool isPatchRequest)
        {
            try
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                if (isPatchRequest)
                {
                    requestMessage.Headers.Add("X-HTTP-Method-Override", "PATCH");
                }
                string jsonRawData = await client.SendAsync(requestMessage).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult().Content.ReadAsStringAsync();

                return jsonRawData;
            }
            catch (Exception ex3)
            {
                return JsonConvert.SerializeObject(ex3.Message);
            }
        }
        #endregion
    }
}
