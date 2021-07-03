using System;
using System.Collections.Generic;
using Runescape.Api;
using Runescape.Api.Model;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Text;

namespace RunescapeAPITest
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }





    class Program
    {
       // public TResponse CreateHttpRequestAsync<TRequest, TResponse>(Uri uri, TRequest request)
       //where TRequest : class
       //where TResponse : class, new()
       // {
       //     using (var client = new HttpClient())
       //     {
       //         client.DefaultRequestHeaders.Accept.Clear();
       //         client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
       //         client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
       //         //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
       //         var response = client.PostAsJsonAsync(uri, request);
       //         if (response.Result.StatusCode == HttpStatusCode.OK)
       //         {
       //             var json = response.Result.Content.ReadAsStringAsync();
       //             return JsonConvert.DeserializeObject<TResponse>(json.Result);
       //         }
       //         else if (response.Result.StatusCode == HttpStatusCode.Unauthorized)
       //         {
       //             throw new Exception();
       //         }
       //         else
       //         {
       //             throw new InvalidOperationException();
       //         }
       //     }

       // }
        //static HttpClient client = new HttpClient();
        //static async Task<Uri> CreateProductAsync(Product product)
        //{
        //    HttpResponseMessage response = await client.PostAsJsonAsync(
        //        "api/products", product);
        //    response.EnsureSuccessStatusCode();

        //    // return URI of the created resource.
        //    return response.Headers.Location;
        //}

        static async Task Main(string[] args)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();


            string url = "https://secure.runescape.com";

            // create a request
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(url); request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";


            // turn our request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes("{url: \"https://secure.runescape.com/m=website-data/playerDetails.ws?names=[%22Name%22]\",  dataType: \"jsonp\"}");

            // this is important - make sure you specify type this way
            request.ContentType = "application/json; charset=UTF-8";
            request.Accept = "application/json";
            request.ContentLength = postBytes.Length;
            //request.CookieContainer = Cookies;
            //request.UserAgent = currentUserAgent;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab te response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
            {
                result = rdr.ReadToEnd();
            }




            //HttpResponseMessage response = await client.PostAsJsonAsync("https://secure.runescape.com/m=website-data/playerDetails.ws?names=[%22feerip%22]", "feerip");

            //https://secure.runescape.com/m=website-data/playerDetails.ws?names=[%22Name%22]


            stopwatch.Stop();
            TimeSpan ts1 = stopwatch.Elapsed;







            //Console.WriteLine($"Total time to pull and filter all vought player adventure logs from Runemetrics: {ts1.TotalSeconds} seconds.");

            Console.WriteLine();
        }

    }
}
