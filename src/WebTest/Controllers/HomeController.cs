using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
            return Redirect("https://accounts.google.com/o/oauth2/v2/auth?redirect_uri=https://localhost:44366/home/authentication&prompt=consent&response_type=code&client_id=862194400783-128gj3m1j52gs6lrl6ueeehtgaiqq8q8.apps.googleusercontent.com&scope=profile+https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email&access_type=offline");
        }

        public IActionResult Authentication(string code)
        {
            using (var client = new HttpClient())
            {
                var jwtToken = GetGoogleToken(code, client);

                if (!string.IsNullOrEmpty(jwtToken))
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
            var payload = new
            {
                code = code,
                grant_type = "authorization_code",
                client_id = "862194400783-128gj3m1j52gs6lrl6ueeehtgaiqq8q8.apps.googleusercontent.com",
                client_secret = "DSSIWDRHxViWR8ssUCLd1WaC",
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
