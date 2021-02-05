using Hanssens.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebTest.Models;

namespace WebTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index(string entryId)
        {
            return View(entryId);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult GoogleAuth()
        {
            string clientId = _config.GetSection("GoogleData").GetSection("ClientId").Value;
            string clientSecret = _config.GetSection("GoogleData").GetSection("ClientSecret").Value;
            string redirectUrl = "https://localhost:44366/home/authentication";

            var url = $"https://accounts.google.com/o/oauth2/v2/auth?redirect_uri={redirectUrl}&prompt=consent&response_type=code&client_id={clientId}&scope=profile+https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email&access_type=offline";
            return Redirect(url);
        }

        public IActionResult Authentication(string code)
        {
            var storage = new LocalStorage();
            if (!storage.Exists("jwtToken") || string.IsNullOrEmpty(storage.Get("jwtToken").ToString()))
            {
                using (var client = new HttpClient())
                {
                    var jwtToken = GetGoogleToken(code, client);
                    storage.Store("jwtToken", jwtToken);
                    storage.Persist();
                }
            }

            return Redirect("Index");
        }

        public IActionResult Logout()
        {
            var storage = new LocalStorage();
            if (storage.Exists("jwtToken"))
            {
                storage.Store("jwtToken", "");
                storage.Persist();
            }

            return Redirect("Index");
        }

        public IActionResult CreateEntry()
        {
            var storage = new LocalStorage();
            var jwtToken = storage.Get("jwtToken").ToString();

            if (!string.IsNullOrEmpty(jwtToken))
            {
                using (var client = new HttpClient())
                {
                    var newEntry = new
                    {
                        tags = new string[] { "chess", "formula1" }

                    };

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var responseCreateEntry = client.PostAsJsonAsync("http://localhost:51665/entry", newEntry).Result;

                    if (responseCreateEntry.IsSuccessStatusCode)
                    {
                        //var createdEntryId = responseCreateEntry.Content.ReadFromJsonAsync<string>().Result;
                        //ViewBag.CreatedEntryId = createdEntryId;
                    }
                }
            }

            return View("Index");
        }

        private string GetGoogleToken(string code, HttpClient client)
        {
            string clientId = _config.GetSection("GoogleData").GetSection("ClientId").Value;
            string clientSecret = _config.GetSection("GoogleData").GetSection("ClientSecret").Value;

            var payload = new
            {
                code = code,
                grant_type = "authorization_code",
                client_id = clientId,
                client_secret = clientSecret,
                redirect_uri = "https://localhost:44366/home/authentication"
            };

            var responseGoogleAuth = client.PostAsJsonAsync("https://oauth2.googleapis.com/token", payload).Result;

            if (responseGoogleAuth.IsSuccessStatusCode)
            {
                var authData = responseGoogleAuth.Content.ReadFromJsonAsync<GoogleAuthData>().Result;
                return authData.id_token;
            }
            return "";
        }
    }
}
