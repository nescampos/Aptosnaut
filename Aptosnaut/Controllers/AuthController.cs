using Aptos;
using Aptosnaut.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OneOf.Types;
using System;
using System.Configuration;

namespace Aptosnaut.Controllers
{
    public class AuthController : BaseController
    {
        private IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string provider)
        {
            var ephemeralKeyPair = EphemeralKeyPair.Generate();
            var userId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("aptosnautUserId", userId);
            ekpDict.Add(userId, ephemeralKeyPair);

            var nonce = ephemeralKeyPair.Nonce;
            var clientId = _configuration["GoogleClientId"];
            var redirectURI = "https://localhost:7067/Auth/LoginGooglePostback";

            var queries = new Dictionary<string, string>();
            queries.Add("nonce", nonce);
            queries.Add("client_id", clientId);
            queries.Add("redirect_uri", redirectURI);
            queries.Add("response_type", "id_token");
            queries.Add("scope", "openid email profile");

            var urlBuilder = new UriBuilder("https://accounts.google.com/o/oauth2/v2/auth")
            {
                Query = string.Join("&", queries.Select(kvp => $"{kvp.Key}={kvp.Value}"))
            };
            
            return Redirect(urlBuilder.Uri.AbsoluteUri);
        }

        public IActionResult LoginGooglePostback()
        {
            return View();
        }

        public async Task<IActionResult> DeriveAccount(string id_token)
        {
            if(string.IsNullOrEmpty(id_token))
            {
                return RedirectToAction("Login");
            }
            string userId = HttpContext.Session.GetString("aptosnautUserId");
            var ephemeralKeyPair = ekpDict[userId];
            var keylessAccount = await client.Keyless.DeriveAccount(id_token, ephemeralKeyPair);
            keylessAccounts.Add(userId, keylessAccount);
            HttpContext.Session.SetString("aptosnaut_token", id_token);
            return RedirectToAction("Index", "Wallet");

        }

        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                string userId = HttpContext.Session.GetString("aptosnautUserId");
                HttpContext.Session.Remove("aptosnautUserId");
                ekpDict.Remove(userId);
                keylessAccounts.Remove(userId);
            }
            catch
            {

            }
            return RedirectToAction("Index");
        }
    }
}
