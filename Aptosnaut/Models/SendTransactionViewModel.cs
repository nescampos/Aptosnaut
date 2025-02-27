using Aptos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Aptosnaut.Models
{
    public class SendTransactionViewModel
    {
        public SendTransactionFormModel Form { get; set; }
        public ulong? TotalFee { get; set; }
        public IEnumerable<SelectListItem> Assets { get; set; }

        public SendTransactionViewModel()
        {
            
        }
    }
}
