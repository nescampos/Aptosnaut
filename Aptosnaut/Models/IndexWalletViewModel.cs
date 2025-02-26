using Aptos;

namespace Aptosnaut.Models
{
    public class IndexWalletViewModel
    {
        public string Address { get; set; }
        public bool IsSpecial { get; set; }
        public decimal APTAmount { get; set; }

        public IEnumerable<AptosBalance> Balances { get; set; }
        public IEnumerable<AptosResource> Resources { get; set; }
        public string? AuthenticationKey { get; set; }
        public ulong? SequenceNumber { get; set; }
    }
}
