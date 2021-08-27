using GuavaStart.Globals;
using GuavaStart.Messages;
using GuavaStart.Models;
using GuavaStart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GuavaStart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class GuavaStockController
    {
            
        public GuavaStockController()
        {             
            
        }

        /// <summary>
        /// Get autorized user`s holded stosks with calculated profit
        /// </summary>
        /// <response code="200">Stocks json</response>        
        /// <response code="404">Service unavailable</response>
        [HttpGet]
        [Route("getuserstocks")]
        public IActionResult GetUserStocks()
        {
            var request = WebRequest.Create("http://guavastockservice:1782/getuserstocks|"+GlobalVars.CurrentUserId.ToString());
            request.Method = "GET";
            request.Timeout = 2000;
            try
            {
                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();
                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();
                return new JsonResult(data);
            }
            catch (Exception)
            {
                return new NotFoundResult();
            }

        }

        /// <summary>
        /// Get USD Market stosks with rates history for 60 days
        /// </summary>
        /// <response code="200">Stocks json</response>        
        /// <response code="404">Service unavailable</response>
        [HttpGet]
        [Route("getusdstocks")]
        public IActionResult GetUSDStocks()
        {
            var request = WebRequest.Create("http://mockstockserver:1783/GetUSDStockData|");
            request.Method = "GET";
            request.Timeout = 2000;
            try
            {
                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();
                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();
                return new JsonResult(data);
            }
            catch (Exception)
            {
                return new NotFoundResult();
            }
           
        }

        /// <summary>
        /// Get AED Market stosks with rates history for 60 days
        /// </summary>
        /// <response code="200">Stocks json</response>        
        /// <response code="404">Service unavailable</response>
        [HttpGet]
        [Route("getaedstocks")]
        public IActionResult GetAEDStocks()
        {
            var request = WebRequest.Create("http://mockstockserver:1783/GetAEDStockData|");
            request.Method = "GET";
            request.Timeout = 2000;

            try
            {
                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();

                return new JsonResult(data);
            }
            catch (Exception)
            {

                return new NotFoundResult();
            }
            
        }

        /// <summary>
        /// Get EUR Market stosks with rates history for 60 days
        /// </summary>
        /// <response code="200">Stocks json</response>        
        /// <response code="404">Service unavailable</response>
        [HttpGet]
        [Route("geteurstocks")]
        public IActionResult GetEURStocks()
        {
            var request = WebRequest.Create("http://mockstockserver:1783/GetEURStockData|");
            request.Method = "GET";
            request.Timeout = 2000;

            try
            {
                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();

                return new JsonResult(data);
            }
            catch (Exception)
            {
                return new NotFoundResult();
            }

        }

        /// <summary>
        /// Get autorized user's preferential stocks list
        /// </summary>
        /// <response code="200">Stocks json</response>        
        /// <response code="404">Service unavailable</response>
        [HttpGet]
        [Route("getprefentialstocks")]
        public IActionResult GetPrefStocks()
        {
            var request = WebRequest.Create("http://mockstockserver:1783/GetPrefStockData|");
            request.Method = "GET";
            request.Timeout = 2000;

            try
            {
                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();

                return new JsonResult(data);
            }
            catch (Exception)
            {
                return new NotFoundResult();
            }

        }

        /// <summary>
        /// Begins buying transaction for autorized user for deticated price
        /// </summary>
        [HttpPost]
        [Route("buystock")]
        public async Task<ActionResult<string>> BuyStock(string StockID, decimal maxprice)
        {
            StockTransaction transaction = new StockTransaction
            {
                hookURL = "http://guavastockservice:1782/",
                userId = GlobalVars.CurrentUserId,
                maxPrice = maxprice,
                stockID = StockID
            };

            var json = JsonConvert.SerializeObject(transaction);
            var content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    client.DefaultRequestHeaders.Add("subject", "trybuystock");
                    var result = await client.PostAsync("http://guavastockservice:1782/", content);
                    var converted = await result.Content.ReadAsStringAsync();                    
                    return converted;
                }
            }
        }

        /// <summary>
        /// Begins selling transaction for autorized user for deticated price
        /// </summary>
        [HttpPost]
        [Route("sellstock")]
        public async Task<ActionResult<string>> SellStock (int SellingGuavaStockID, decimal sellprice)
        {
            SellingStockTransaction transaction = new SellingStockTransaction
            {
                hookURL = "http://guavastockservice:1782/",
                userId = GlobalVars.CurrentUserId,
                maxPrice = sellprice,                
                holdID = SellingGuavaStockID
            };

            var json = JsonConvert.SerializeObject(transaction);
            var content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    client.DefaultRequestHeaders.Add("subject", "trysellstock");
                    var result = await client.PostAsync("http://guavastockservice:1782/", content);
                    var converted = await result.Content.ReadAsStringAsync();
                    return converted;
                }
            }
        }
    }
}
