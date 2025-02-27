using Aptos;
using Microsoft.AspNetCore.Mvc;

namespace Aptosnaut.Controllers
{
    public class BaseController : Controller
    {
        protected AptosClient client = new AptosClient(Networks.Mainnet);
        protected static Dictionary<string, EphemeralKeyPair> ekpDict = new Dictionary<string, EphemeralKeyPair>();
        protected static Dictionary<string, KeylessAccount> keylessAccounts = new Dictionary<string, KeylessAccount>();
    }
}
