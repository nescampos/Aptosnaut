using Aptos;
using Aptosnaut.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace Aptosnaut.Controllers
{
    public class WalletController : BaseController
    {
        //private AptosClient client = null;
        //public WalletController()
        //{
        //    client = new AptosClient(Networks.Mainnet);
        //}
        public async Task<IActionResult> Index()
        {
            string userId = HttpContext.Session.GetString("aptosnautUserId");
            var ephemeralKeyPair = ekpDict[userId];
            var keylessAccount = keylessAccounts[userId];
            var keylessAccountBalance = await client.Account.GetCoinBalance(keylessAccount.Address);
            var keylessAccountBalances = await client.Account.GetCoinBalances(keylessAccount.Address);
            IndexWalletViewModel model = new IndexWalletViewModel
            {
                Address = keylessAccount.Address.ToStringLong(), IsSpecial = keylessAccount.Address.IsSpecial()
            };
            if(keylessAccountBalance != null)
            {
                decimal power = (decimal)Math.Pow(10, 8);
                model.APTAmount = keylessAccountBalance.Amount.GetValueOrDefault() / power;
            }
            if(keylessAccountBalances != null && keylessAccountBalances.Any())
            {
                keylessAccountBalances.Select(x => x.AssetType)

            }
            return View(model);
        }

        public IActionResult SendTransaction()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTransaction(string address, decimal amount)
        {
            string userId = HttpContext.Session.GetString("aptosnautUserId");
            var keylessAccount = keylessAccounts[userId];
            var txn = await client.Transaction.Build(
                sender: keylessAccount,
                data: new GenerateEntryFunctionPayloadData(
                    function: "0x1::aptos_account::transfer_coins",
                    typeArguments: ["0x1::aptos_coin::AptosCoin"],
                    functionArguments: [address, amount]
                )
            );
            var pendingTxn = await client.Transaction.SignAndSubmitTransaction(keylessAccount, txn);
            var committedTxn = await client.Transaction.WaitForTransaction(pendingTxn.Hash);
            if (committedTxn.Success)
            {
                return RedirectToAction("TransactionSent", new { id = pendingTxn.Hash });
            }
            else
            {
                return RedirectToAction("TransactionError", new { id = pendingTxn.Hash });
            }
        }
    }
}
