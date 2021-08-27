using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using GuavaStockService.Services;
using GuavaStockService.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GuavaStockService
{
    class Program
    {
        private static readonly HttpListener listener = new HttpListener();
        static List<StockTransaction> transactionsqueue = new List<StockTransaction>();
        private static Timer timer;

        static void Main(string[] args)
        {
            listener.Prefixes.Add("http://*:1782/");
            listener.Start();
            StartServer();
            Console.WriteLine("Starting StockService");

            timer = new Timer(10000); // 1 seconds
            timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            timer.Interval = 10000;
            timer.Enabled = true;
            timer.Start();

            while (true)
            {

            }
        }

        private static void OnTimerElapsed(object source, ElapsedEventArgs e)
        {

            if (transactionsqueue.Any())
            {
                Console.WriteLine("Working on transaction in queue");
                foreach (var item in transactionsqueue)
                {
                    TryBuyStock(item);
                }
            }

        }
        private static async void StartServer()
        {
            while (true)
            {
                var context = await listener.GetContextAsync();
                _ = Task.Factory.StartNew(() => ProcessRequest(context));
            }
        }

        private async static void ProcessRequest(HttpListenerContext context)
        {

            var request = context.Request;
            string[] separatingChars = { "|" };
            string replacestring = request.RawUrl.Replace("%20", " ");
            replacestring = replacestring.Replace("%7C", "|");
            replacestring = replacestring.Remove(0, 1);

            Console.WriteLine("Request recieved" + request.HttpMethod);
            if (request.HttpMethod == "GET")
            {
                string[] requeststring = replacestring.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                if (requeststring[0] == "getuserstocks")
                {
                    Random rnd = new Random();
                    if (rnd.Next(0, 5) != 3)
                    {
                        List<GuavaStock> userStocks = new List<GuavaStock>();
                        using (StockDbContext db = new StockDbContext())
                        {
                            userStocks = db.GuavaStocks.Where(i => i.UserID == Convert.ToInt32(requeststring[1])).ToList();                            
                            Console.WriteLine($"DB Found {userStocks.Count()} stocks");
                        }

                        foreach (var item in userStocks)
                        {
                            GetCurrentPrice(item);
                        }

                        string resp = JsonConvert.SerializeObject(userStocks);
                        byte[] buffer = Encoding.UTF8.GetBytes(resp);
                        Stream output = context.Response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                }               
            }
           
            if (request.HttpMethod == "POST")
            {
                try
                {
                    #region postdeserializer
                    var inputStream = context.Request.InputStream;
                    string responceString = "Error";
                    string headers = "";
                    for (int i = 0; i < context.Request.Headers.Count; i++)
                    {
                        headers += context.Request.Headers[i] + " ";
                    }

                    byte[] buffer = new byte[1024];
                    int totalCount = 0;
                    while (true)
                    {
                        int currentCount = inputStream.Read(buffer, totalCount, buffer.Length - totalCount);
                        if (currentCount == 0)
                            break;
                        totalCount += currentCount;
                        if (totalCount == buffer.Length)
                            Array.Resize(ref buffer, buffer.Length * 2);
                    }
                    Array.Resize(ref buffer, totalCount);
                    MemoryStream memStream = new MemoryStream();
                    memStream.Write(buffer, 0, buffer.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    #endregion

                    using (memStream)
                    {
                        if (headers.Contains("buyoperationsuccess"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var guavaStock = JsonConvert.DeserializeObject<GuavaStock>(req);
                            Console.WriteLine($"{guavaStock.OperationDate} {guavaStock.UserID} {guavaStock.StockName} {guavaStock.OperationPrice} {guavaStock.OperationVolume}");
                            using (StockDbContext db = new StockDbContext())
                            {
                                db.GuavaStocks.Add(guavaStock);
                                db.SaveChanges();
                                var stocks = db.GuavaStocks.ToList();
                                for (int i = 0; i < stocks.Count; i++)
                                {
                                    Console.WriteLine($"DB {stocks[i].OperationDate} {stocks[i].UserID} {stocks[i].StockName} {stocks[i].OperationPrice} {stocks[i].OperationVolume}");
                                }
                            }

                            string resp = "Success stored";
                            Console.WriteLine(resp);
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();                           
                            SendNotification(guavaStock.UserID, resp);
                        }

                        if (headers.Contains("buypriceerror"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var guavaStock = JsonConvert.DeserializeObject<GuavaStock>(req);
                            Console.WriteLine($"Price Error on {guavaStock.OperationDate} {guavaStock.UserID} {guavaStock.StockName} {guavaStock.OperationPrice} {guavaStock.OperationVolume}");                            

                            string resp = "Price Error";
                            Console.WriteLine(resp);
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();                           
                            SendNotification(guavaStock.UserID, resp);
                        }

                        if (headers.Contains("buystockerror"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var guavaStock = JsonConvert.DeserializeObject<GuavaStock>(req);
                            Console.WriteLine($"Price Error on {guavaStock.OperationDate} {guavaStock.UserID} {guavaStock.StockName} {guavaStock.OperationPrice} {guavaStock.OperationVolume}");

                            string resp = "Stock Error";
                            Console.WriteLine(resp);
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();                           
                            SendNotification(guavaStock.UserID, resp);
                        }

                        if (headers.Contains("trybuystock"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var stockTransaction = JsonConvert.DeserializeObject<StockTransaction>(req);
                            Console.WriteLine($"{stockTransaction.stockID} {stockTransaction.userId} {stockTransaction.maxPrice} {stockTransaction.hookURL}");  
                            string resp = "Transaction processing";
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();
                            TryBuyStock(stockTransaction);
                        }

                        if (headers.Contains("trysellstock"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var stockTransaction = JsonConvert.DeserializeObject<SellingStockTransaction>(req);
                            using (StockDbContext db = new StockDbContext())
                            {
                                var targetStock = db.GuavaStocks.Where(i => i.Id == stockTransaction.holdID).First();
                                stockTransaction.stockID = targetStock.StockId;                               
                            }
                            Console.WriteLine($"{stockTransaction.stockID} {stockTransaction.holdID} {stockTransaction.userId} {stockTransaction.maxPrice} {stockTransaction.hookURL}");
                            string resp = "Transaction processing";
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();
                            TrySellStock(stockTransaction);
                        }

                        if (headers.Contains("selloperationsuccess"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var soldStock = JsonConvert.DeserializeObject<SellingStockTransaction>(req);
                          
                            using (StockDbContext db = new StockDbContext())
                            {
                                var target = db.GuavaStocks.Where(i => i.Id == soldStock.holdID).First();
                                db.GuavaStocks.Remove(target);
                                db.SaveChanges();
                                Console.WriteLine($"DB Removing {target.OperationDate} {target.UserID} {target.StockName} {target.OperationPrice} {target.OperationVolume}");                                
                            }

                            string resp = "Stock sold";
                            Console.WriteLine(resp);
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();
                            SendNotification(soldStock.userId, resp);
                        }

                        if (headers.Contains("sellpriceerror"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var soldStock = JsonConvert.DeserializeObject<SellingStockTransaction>(req);
                            Console.WriteLine($"Price Error on {soldStock.holdID}");

                            string resp = "Selling Price Error";
                            Console.WriteLine(resp);
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();
                            SendNotification(soldStock.userId, resp);
                        }

                        if (headers.Contains("sellstockerror"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var soldStock = JsonConvert.DeserializeObject<SellingStockTransaction>(req);
                            Console.WriteLine($"Price Error on {soldStock.holdID}");

                            string resp = "Selling Stock Error";
                            Console.WriteLine(resp);
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();
                            SendNotification(soldStock.userId, resp);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public async static void TryBuyStock(StockTransaction stockTransaction)
        {
            if (transactionsqueue.Contains(stockTransaction))
            {
                transactionsqueue.Remove(stockTransaction);
            }

            var json = JsonConvert.SerializeObject(stockTransaction);
            var content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        client.DefaultRequestHeaders.Add("subject", "buystock");
                        var result = await client.PostAsync("http://mockstockserver:1783/", content);
                        var converted = await result.Content.ReadAsStringAsync();
                        Console.WriteLine(converted);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error while connecting stock service. Adding transaction to queue");
                transactionsqueue.Add(stockTransaction);
            }
            
        }

        public async static void TrySellStock(SellingStockTransaction stockTransaction)
        {
            var json = JsonConvert.SerializeObject(stockTransaction);
            var content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    client.DefaultRequestHeaders.Add("subject", "sellstock");
                    var result = await client.PostAsync("http://mockstockserver:1783/", content);
                    var converted = await result.Content.ReadAsStringAsync();
                    Console.WriteLine(converted);
                }
            }
        }

        public static void SendNotification(int userId, string message)
        {
            var request = WebRequest.Create("http://guavastart:80/guavanotification/getwsuserid");
            request.Method = "GET";
            request.Timeout = 200;

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            int userID = Convert.ToInt32(data);

            if (userID  == userId)
            {
            NotificationMessenger notificationMessenger = new NotificationMessenger(message);
            }
            else
            {
                SendEmail(userId, message);
            }



        }

        public static void GetCurrentPrice(GuavaStock guavaStock)
        {
            var request = WebRequest.Create("http://mockstockserver:1783/GetCurrentPrice|"+guavaStock.StockId);
            request.Method = "GET";
            request.Timeout = 500;

            try
            {
                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();
                guavaStock.CurrentPrice = Convert.ToDecimal(data);
                guavaStock.Profit = (Convert.ToDecimal(data) * 100 / guavaStock.OperationPrice) - 100;

            }
            catch (Exception)
            {
                
            }
        }

        public static void SendEmail(int userId, string message)
        {
            Console.WriteLine("Sending Mail to " + userId);
        }
    }
}


