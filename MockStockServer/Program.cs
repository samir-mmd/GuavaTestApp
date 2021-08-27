using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Threading;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MockStockServer
{
    class Program
    {
        
        private static readonly HttpListener listener = new HttpListener();
        static List<StockBase> stocksUSD = new List<StockBase>();
        static List<StockBase> stocksAED = new List<StockBase>();
        static List<StockBase> stocksEUR = new List<StockBase>();       
        static List<decimal> values = new List<decimal>();
        public static Timer timer;

        static void Main(string[] args)
        {

            MockStockGenerator();
            listener.Prefixes.Add("http://*:1783/");
            Console.WriteLine("Starting MockStockServer");
            listener.Start();
            StartServer();

            timer = new System.Timers.Timer(10000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            while (true)
            {

            }
            Console.WriteLine("Exiting");
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Updating Current Prices");
            UpdateCurrentPrice();
        }

        public static void MockStockGenerator()
        {            
            stocksUSD.Add(new StockBase("USD","AAPL", "Apple Inc. Common Stock"));
            stocksUSD.Add(new StockBase("USD","ADBE", "Adobe Inc. Common Stock"));
            stocksUSD.Add(new StockBase("USD","ADSK", "Autodesk Inc. Common Stock"));
            stocksUSD.Add(new StockBase("USD","AMZN", "Amazon.com Inc. Common Stock"));
            stocksUSD.Add(new StockBase("USD","ASML", "ASML Holding N.V. New York Registry Shares"));
            stocksUSD.Add(new StockBase("USD","CMCSA", "Comcast Corporation Class A Common Stock"));
            stocksUSD.Add(new StockBase("USD","COST", "Costco Wholesale Corporation Common Stock"));
            stocksUSD.Add(new StockBase("USD","CSCO", "Cisco Systems Inc. Common Stock (DE)"));
            stocksUSD.Add(new StockBase("USD","FB", "Facebook Inc. Class A Common Stock"));
            stocksUSD.Add(new StockBase("USD","GOOG", "Alphabet Inc. Class C Capital Stock"));
            stocksUSD.Add(new StockBase("USD","GOOGL", "Alphabet Inc. Class A Common Stock"));
            stocksUSD.Add(new StockBase("USD","INTC", "Intel Corporation Common Stock"));
            stocksUSD.Add(new StockBase("USD","MSFT", "Microsoft Corporation Common Stock"));
            stocksUSD.Add(new StockBase("USD","NFLX", "Netflix Inc. Common Stock"));
            stocksUSD.Add(new StockBase("USD","NVDA", "NVIDIA Corporation Common Stock"));
            stocksUSD.Add(new StockBase("USD","PEP", "PepsiCo Inc. Common Stock"));
            stocksUSD.Add(new StockBase("USD","PYPL", "PayPal Holdings Inc. Common Stock"));
            stocksUSD.Add(new StockBase("USD","TSLA", "Tesla Inc. Common Stock"));

            stocksAED.Add(new StockBase("AED","AAPL", "Apple Inc. Common Stock"));
            stocksAED.Add(new StockBase("AED","ADBE", "Adobe Inc. Common Stock"));
            stocksAED.Add(new StockBase("AED","ADSK", "Autodesk Inc. Common Stock"));
            stocksAED.Add(new StockBase("AED","AMZN", "Amazon.com Inc. Common Stock"));
            stocksAED.Add(new StockBase("AED","ASML", "ASML Holding N.V. New York Registry Shares"));
            stocksAED.Add(new StockBase("AED","CMCSA", "Comcast Corporation Class A Common Stock"));
            stocksAED.Add(new StockBase("AED","COST", "Costco Wholesale Corporation Common Stock"));
            stocksAED.Add(new StockBase("AED","CSCO", "Cisco Systems Inc. Common Stock (DE)"));
            stocksAED.Add(new StockBase("AED","FB", "Facebook Inc. Class A Common Stock"));
            stocksAED.Add(new StockBase("AED","GOOG", "Alphabet Inc. Class C Capital Stock"));
            stocksAED.Add(new StockBase("AED","GOOGL", "Alphabet Inc. Class A Common Stock"));
            stocksAED.Add(new StockBase("AED","INTC", "Intel Corporation Common Stock"));
            stocksAED.Add(new StockBase("AED","MSFT", "Microsoft Corporation Common Stock"));
            stocksAED.Add(new StockBase("AED","NFLX", "Netflix Inc. Common Stock"));
            stocksAED.Add(new StockBase("AED","NVDA", "NVIDIA Corporation Common Stock"));
            stocksAED.Add(new StockBase("AED","PEP", "PepsiCo Inc. Common Stock"));
            stocksAED.Add(new StockBase("AED","PYPL", "PayPal Holdings Inc. Common Stock"));
            stocksAED.Add(new StockBase("AED","TSLA", "Tesla Inc. Common Stock"));

            stocksEUR.Add(new StockBase("EUR","AAPL", "Apple Inc. Common Stock"));
            stocksEUR.Add(new StockBase("EUR","ADBE", "Adobe Inc. Common Stock"));
            stocksEUR.Add(new StockBase("EUR","ADSK", "Autodesk Inc. Common Stock"));
            stocksEUR.Add(new StockBase("EUR","AMZN", "Amazon.com Inc. Common Stock"));
            stocksEUR.Add(new StockBase("EUR","ASML", "ASML Holding N.V. New York Registry Shares"));
            stocksEUR.Add(new StockBase("EUR","CMCSA", "Comcast Corporation Class A Common Stock"));
            stocksEUR.Add(new StockBase("EUR","COST", "Costco Wholesale Corporation Common Stock"));
            stocksEUR.Add(new StockBase("EUR","CSCO", "Cisco Systems Inc. Common Stock (DE)"));
            stocksEUR.Add(new StockBase("EUR","FB", "Facebook Inc. Class A Common Stock"));
            stocksEUR.Add(new StockBase("EUR","GOOG", "Alphabet Inc. Class C Capital Stock"));
            stocksEUR.Add(new StockBase("EUR","GOOGL", "Alphabet Inc. Class A Common Stock"));
            stocksEUR.Add(new StockBase("EUR","INTC", "Intel Corporation Common Stock"));
            stocksEUR.Add(new StockBase("EUR","MSFT", "Microsoft Corporation Common Stock"));
            stocksEUR.Add(new StockBase("EUR","NFLX", "Netflix Inc. Common Stock"));
            stocksEUR.Add(new StockBase("EUR","NVDA", "NVIDIA Corporation Common Stock"));
            stocksEUR.Add(new StockBase("EUR","PEP", "PepsiCo Inc. Common Stock"));
            stocksEUR.Add(new StockBase("EUR","PYPL", "PayPal Holdings Inc. Common Stock"));
            stocksEUR.Add(new StockBase("EUR","TSLA", "Tesla Inc. Common Stock"));
            MockRandomStockUpdate();
        }

        public static void MockRandomStockUpdate()
        {
            for (int i = 0; i < 150000; i++)
            {
                Random rng = new Random();               
                decimal value = Convert.ToDecimal(((rng.Next(10, 34000) * 0.01) * rng.NextDouble()));
                if (value>0)
                {
                    values.Add(Math.Round(value,3));
                }
            }

            values = values.OrderBy(v=>v).ToList();
           

            for (int i = 0; i < stocksUSD.Count; i++)
            {
                stocksUSD[i].dailyInfos.Clear();
                stocksAED[i].dailyInfos.Clear();
                stocksEUR[i].dailyInfos.Clear();

                Random rng = new Random();                
                decimal high = values.Max()* ((i+1) * (i+2)) / 100m;
                decimal low = PickRandomDecimal(values, high*0.93m, high);
                decimal open = PickRandomDecimal(values, low, high);
                decimal close = PickRandomDecimal(values, low, high);

                for (int d = 0; d < 60; d++)
                {                    
                    Random drng = new Random();
                    decimal currentHigh = high * (drng.Next(100, 102) / 100m);
                    decimal currentLow = low * (drng.Next(98, 100) / 100m);                
                    var stockInfoUSD = new StockDailyInfo
                    {
                        DateStamp = DateTime.Now.AddDays(-d),
                        high = currentHigh,
                        open = PickRandomDecimal(values, currentLow, currentHigh),
                        low = currentLow,
                        close = PickRandomDecimal(values, currentLow, currentHigh),
                        volume = drng.Next(100, 2540) * i / (d + 1)
                    };
                    stocksUSD[i].dailyInfos.Add(stockInfoUSD);

                    decimal aedMultiplier = (drng.Next(99, 101) / 100m * 3.67m);
                    var stockInfoAED = new StockDailyInfo
                    {
                        DateStamp = DateTime.Now.AddDays(-d),
                        high = stockInfoUSD.high * aedMultiplier,
                        open = stockInfoUSD.open * aedMultiplier,
                        low = stockInfoUSD.low * aedMultiplier,
                        close = stockInfoUSD.close * aedMultiplier,
                        volume = drng.Next(100, 2540) * i / (d + 1)
                    };

                    stocksAED[i].dailyInfos.Add(stockInfoAED);

                    decimal eurMultiplier = (drng.Next(99, 101) / 100m * 0.85m);
                    var stockInfoEUR = new StockDailyInfo
                    {
                        DateStamp = DateTime.Now.AddDays(-d),
                        high = stockInfoUSD.high * eurMultiplier,
                        open = stockInfoUSD.open * eurMultiplier,
                        low = stockInfoUSD.low * eurMultiplier,
                        close = stockInfoUSD.close * eurMultiplier,
                        volume = drng.Next(100, 2540) * i / (d + 1)
                    };

                    stocksEUR[i].dailyInfos.Add(stockInfoAED);                   
                }              
            }
            UpdateCurrentPrice();
        }

        public static void UpdateCurrentPrice()
        {
            for (int i = 0; i < stocksUSD.Count; i++)
            {                
                stocksUSD[i].CurrentPrice = PickRandomDecimal(values, stocksUSD[i].dailyInfos.First().low, stocksUSD[i].dailyInfos.First().high);
                stocksAED[i].CurrentPrice = PickRandomDecimal(values, stocksAED[i].dailyInfos.First().low, stocksAED[i].dailyInfos.First().high);
                stocksEUR[i].CurrentPrice = PickRandomDecimal(values, stocksEUR[i].dailyInfos.First().low, stocksEUR[i].dailyInfos.First().high);
            }
        }

        public static decimal PickRandomDecimal(List<decimal> decimals, decimal start, decimal end)
        {
            List<decimal> workArea = decimals.Where(x => x >= start && x <= end).ToList();

            if (workArea.Count>2)
            {
                Random random = new Random();

                return workArea[random.Next(0, workArea.Count - 1)];
            }
            else
            {
                return (start + end) / 2;
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
          
            Console.WriteLine("Request recieved");
            var request = context.Request;
            string[] separatingChars = { "|" };
            string replacestring = request.RawUrl.Replace("%20", " ");
            replacestring = replacestring.Replace("%7C", "|");
            replacestring = replacestring.Remove(0, 1);
            if (request.HttpMethod == "GET")
            {                
                string[] requeststring = replacestring.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                if (requeststring[0] == "GetUSDStockData")
                {
                    Random rnd = new Random();
                    if (rnd.Next(0,5)!=3)
                    {
                        string resp = JsonConvert.SerializeObject(stocksUSD);
                        byte[] buffer = Encoding.UTF8.GetBytes(resp);
                        Stream output = context.Response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                }
                if (requeststring[0] == "GetAEDStockData")
                {
                    Random rnd = new Random();
                    if (rnd.Next(0, 5) != 3)
                    {
                        string resp = JsonConvert.SerializeObject(stocksAED);
                        byte[] buffer = Encoding.UTF8.GetBytes(resp);
                        Stream output = context.Response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                }
                if (requeststring[0] == "GetEURStockData")
                {
                    Random rnd = new Random();
                    if (rnd.Next(0, 5) != 3)
                    {
                        string resp = JsonConvert.SerializeObject(stocksEUR);
                        byte[] buffer = Encoding.UTF8.GetBytes(resp);
                        Stream output = context.Response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                }

                if (requeststring[0] == "GetPrefStockData")
                {
                  
                    var prefbase = stocksUSD.Where(p => p.CurrentPrice> 3).ToList();                
                    
                    prefbase = prefbase.Where(p => p.dailyInfos.Where(d => d.DateStamp > DateTime.Now.AddDays(-5)).ToList().Average(l=>l.low)*1.05m
                    >
                    p.dailyInfos.Where(d => d.DateStamp > DateTime.Now.AddDays(-5)).ToList().Average(h => h.high)).ToList();
                    
                    Console.WriteLine(prefbase.Count() + "Preferential stocks found");
                    string resp = JsonConvert.SerializeObject(prefbase);
                    byte[] buffer = Encoding.UTF8.GetBytes(resp);
                    Stream output = context.Response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }

                if (requeststring[0] == "GetCurrentPrice")      
                {
                    var stock = TryFindStock(requeststring[1]);                    
                    byte[] buffer = Encoding.UTF8.GetBytes(stock.CurrentPrice.ToString());
                    Stream output = context.Response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
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
                        if (headers.Contains("buystock"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var incomingStockTransaction = JsonConvert.DeserializeObject<StockTransaction>(req);                            
                              
                            string resp = "Transaction processing";
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();
                            for (int i = 1; i <= 5; i++)
                            {
                                Thread.Sleep(1000);
                                Console.WriteLine("Processing transaction: " + i);
                            }
                            TryBuyStock(incomingStockTransaction);
                        }

                        if (headers.Contains("sellstock"))
                        {
                            string req = Encoding.UTF8.GetString(memStream.ToArray());
                            var incomingStockTransaction = JsonConvert.DeserializeObject<SellingStockTransaction>(req);

                            string resp = "Transaction processing";
                            byte[] result = Encoding.UTF8.GetBytes(resp);
                            Stream output = context.Response.OutputStream;
                            output.Write(result, 0, result.Length);
                            output.Close();
                            for (int i = 1; i <= 5; i++)
                            {
                                Thread.Sleep(1000);
                                Console.WriteLine("Processing transaction: " + i);
                            }
                            TrySellStock(incomingStockTransaction);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
          
        }

        public static StockBase TryFindStock(string id)
        {
            List<StockBase> tempStocks = new List<StockBase>();
            tempStocks.AddRange(stocksUSD);
            tempStocks.AddRange(stocksAED);
            tempStocks.AddRange(stocksEUR);

            if (tempStocks.Where(i=>i.Id==id).Any())
            {
                return tempStocks.Where(i => i.Id == id).First();
            }
            return null;
        }

        public static async void TryBuyStock(StockTransaction transaction)
        {        
            Console.WriteLine($"Processing transaction id: {transaction.stockID}");
            var stock = TryFindStock(transaction.stockID);
           
            if (stock!=null)
            {
                try
                {
                    Console.WriteLine($"Processing transaction, stock found: {stock.Information} Price: {stock.CurrentPrice}");
                    if (stock.CurrentPrice <= transaction.maxPrice)
                    {
                        Console.WriteLine($"Processing transaction, Current price: {stock.CurrentPrice}, Transaction max price {transaction.maxPrice}");
                        GuavaStock guavaStock = new GuavaStock
                        {
                            UserID = transaction.userId,
                            StockName = stock.Information,
                            StockSymbol = stock.Symbol,
                            OperationDate = DateTime.Now,
                            OperationPrice = stock.CurrentPrice,
                            OperationVolume = 1,
                            StockId = transaction.stockID
                        };

                        var body = JsonConvert.SerializeObject(guavaStock);
                        using (var httpClientHandler = new HttpClientHandler())
                        {
                            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                            using (var client = new HttpClient(httpClientHandler))
                            {
                                Console.WriteLine($"Processing transaction, operation end: sending notification to hook url");
                                client.DefaultRequestHeaders.Add("subject", "buyoperationsuccess");
                                var content = new StringContent(body, UnicodeEncoding.UTF8, "application/json");
                                var result = await client.PostAsync($@"{transaction.hookURL}", content);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Processing transaction, Price error, Current price: {stock.CurrentPrice}, Transaction max price {transaction.maxPrice}");

                        var body = JsonConvert.SerializeObject(transaction);
                        using (var httpClientHandler = new HttpClientHandler())
                        {
                            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                            using (var client = new HttpClient(httpClientHandler))
                            {
                                Console.WriteLine($"Processing transaction, operation end: sending notification to hook url");
                                client.DefaultRequestHeaders.Add("subject", "buypriceerror");
                                var content = new StringContent(body, UnicodeEncoding.UTF8, "application/json");
                                var result = await client.PostAsync($@"{transaction.hookURL}", content);
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
              
            }
            else
            {
                Console.WriteLine($"Coudn't find stock  id: {transaction.stockID}");
                var body = JsonConvert.SerializeObject(transaction);
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        Console.WriteLine($"Processing transaction, operation end: sending notification to hook url");
                        client.DefaultRequestHeaders.Add("subject", "buystockerror");
                        var content = new StringContent(body, UnicodeEncoding.UTF8, "application/json");
                        var result = await client.PostAsync($@"{transaction.hookURL}", content);
                    }
                }
            }
        }

        public static async void TrySellStock(SellingStockTransaction transaction)
        {
            Console.WriteLine($"Processing transaction id: {transaction.stockID}");          
            var stock = TryFindStock(transaction.stockID);

            if (stock != null)
            {
                try
                {
                    Console.WriteLine($"Processing transaction, stock found: {stock.Information} Price: {stock.CurrentPrice}");
                    if (stock.CurrentPrice >= transaction.maxPrice)
                    {
                        Console.WriteLine($"Processing transaction, Current price: {stock.CurrentPrice}, Transaction max price {transaction.maxPrice}");

                        
                        var body = JsonConvert.SerializeObject(transaction);
                        using (var httpClientHandler = new HttpClientHandler())
                        {
                            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                            using (var client = new HttpClient(httpClientHandler))
                            {
                                Console.WriteLine($"Processing transaction, operation end: sending notification to hook url");
                                client.DefaultRequestHeaders.Add("subject", "selloperationsuccess");
                                var content = new StringContent(body, UnicodeEncoding.UTF8, "application/json");
                                var result = await client.PostAsync($@"{transaction.hookURL}", content);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Processing transaction, Price error, Current price: {stock.CurrentPrice}, Transaction max price {transaction.maxPrice}");

                        var body = JsonConvert.SerializeObject(transaction);
                        using (var httpClientHandler = new HttpClientHandler())
                        {
                            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                            using (var client = new HttpClient(httpClientHandler))
                            {
                                Console.WriteLine($"Processing transaction, operation end: sending notification to hook url");
                                client.DefaultRequestHeaders.Add("subject", "sellpriceerror");
                                var content = new StringContent(body, UnicodeEncoding.UTF8, "application/json");
                                var result = await client.PostAsync($@"{transaction.hookURL}", content);
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }

            }
            else
            {
                Console.WriteLine($"Coudn't find stock  id: {transaction.stockID}");
                var body = JsonConvert.SerializeObject(transaction);
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        Console.WriteLine($"Processing transaction, operation end: sending notification to hook url");
                        client.DefaultRequestHeaders.Add("subject", "sellstockerror");
                        var content = new StringContent(body, UnicodeEncoding.UTF8, "application/json");
                        var result = await client.PostAsync($@"{transaction.hookURL}", content);
                    }
                }
            }
        }
    }
}


