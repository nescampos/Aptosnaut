using Aptos;
using Aptosnaut.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Aptosnaut.Controllers
{
    public class WalletController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            string userId = HttpContext.Session.GetString("aptosnautUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }
            //var ephemeralKeyPair = ekpDict[userId];
            var keylessAccount = keylessAccounts[userId];
            var accountInfo = await client.Account.GetInfo(keylessAccount.Address);
            var keylessAccountBalance = await client.Account.GetCoinBalance(keylessAccount.Address);
            //var keylessAccountBalances = await client.Account.GetCoinBalances(keylessAccount.Address);
            var fungibleAssetsBalances = await client.FungibleAsset.GetAccountFungibleAssetBalances(keylessAccount.Address);
            var accountResources = await client.GetResources(keylessAccount.Address);
            IndexWalletViewModel model = new IndexWalletViewModel
            {
                Address = keylessAccount.Address.ToStringLong(), IsSpecial = keylessAccount.Address.IsSpecial()
            };
            if(accountInfo != null)
            {
                model.AuthenticationKey = accountInfo.AuthenticationKey.ToString();
                model.SequenceNumber = accountInfo.SequenceNumber;
            }
            if(keylessAccountBalance != null)
            {
                decimal power = (decimal)Math.Pow(10, 8);
                model.APTAmount = keylessAccountBalance.Amount.GetValueOrDefault() / power;
            }

            if (fungibleAssetsBalances != null && fungibleAssetsBalances.Any())
            {
                model.Balances = fungibleAssetsBalances.Select(x => new AptosBalance { 
                    Amount = (x.Amount.GetValueOrDefault() / (decimal)Math.Pow(10, x.Metadata.Decimals)), 
                    AssetType = x.AssetType, TokenStandard = x.TokenStandard,
                    IsFrozen = x.IsFrozen,
                    Name = x.Metadata.Name,
                    Symbol = x.Metadata.Symbol,
                    Icon = x.Metadata.IconUri
                });
            }
            if (accountResources != null && accountResources.Any())
            {
                model.Resources = accountResources.Select(x => new AptosResource
                {
                    Type = x.Type,
                    Data = x.Data
                });
            }
            return View(model);
        }

        //public IActionResult CreateTransaction()
        //{
        //    SendTransactionViewModel model = new SendTransactionViewModel();
        //    return View(model);

        //}

        //[HttpPost]
        //public IActionResult CreateTransaction(SendTransactionFormModel Form)
        //{
        //    string userId = HttpContext.Session.GetString("aptosnautUserId");
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }
        //    if (!ModelState.IsValid)
        //    {
        //        SendTransactionViewModel model = new SendTransactionViewModel();
        //        model.Form = Form;
        //        return View(model);
        //    }
        //    return RedirectToAction("SendTransaction", new { address = Form.Address, amount = Form.Amount });
        //}

        public async Task<IActionResult> SendTransaction(string? address, ulong? amount)
        {
            string userId = HttpContext.Session.GetString("aptosnautUserId");
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }
            var keylessAccount = keylessAccounts[userId];
            SendTransactionViewModel model = new SendTransactionViewModel();
            var fungibleAssetsBalances = await client.FungibleAsset.GetAccountFungibleAssetBalances(keylessAccount.Address);
            model.Assets = fungibleAssetsBalances.Select(x => new SelectListItem { Text = x.Metadata.Name, Value = x.AssetType });
            //if (address != null && amount.HasValue)
            //{
            //    var keylessAccount = keylessAccounts[userId];
            //    var txn = await client.Transaction.Build(
            //        sender: keylessAccount.Address,
            //        data: new GenerateEntryFunctionPayloadData(
            //            function: "0x1::aptos_account::transfer_coins",
            //            typeArguments: ["0x1::aptos_coin::AptosCoin"],
            //            functionArguments: [address, amount]
            //        )
            //    );
            //    var simulatedTxn = await client.Transaction.Simulate(new(txn, keylessAccount.EphemeralKeyPair.PublicKey));
            //    ulong totalFee = 0;
            //    foreach(var trx in simulatedTxn)
            //    {
            //        totalFee += trx.GasUnitPrice * trx.MaxGasAmount;
            //    }
            //    model.TotalFee = totalFee;
            //}

            model.Form = new SendTransactionFormModel { Address = address, Amount = amount };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendTransaction(SendTransactionFormModel Form)
        {
            string userId = HttpContext.Session.GetString("aptosnautUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }
            var keylessAccount = keylessAccounts[userId];
            if (!ModelState.IsValid)
            {
                SendTransactionViewModel model = new SendTransactionViewModel();
                var fungibleAssetsBalances = await client.FungibleAsset.GetAccountFungibleAssetBalances(keylessAccount.Address);
                model.Assets = fungibleAssetsBalances.Select(x => new SelectListItem { Text = x.Metadata.Name, Value = x.AssetType });
                model.Form = Form;
                return View(model);
            }
            
            var txn = await client.Transaction.Build(
                sender: keylessAccount,
                data: new GenerateEntryFunctionPayloadData(
                    function: "0x1::aptos_account::transfer_coins",
                    typeArguments: [Form.Token],
                    functionArguments: [Form.Address, Form.Amount]
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

        public async Task<IActionResult> TransactionSent(string id)
        {
            TransactionResponse txnResponse = await client.Transaction.GetTransactionByHash(id);
            return View(txnResponse);
        }

        public async Task<IActionResult> TransactionError(string id)
        {
            TransactionResponse txnResponse = await client.Transaction.GetTransactionByHash(id);
            return View(txnResponse);
        }
    }
}
