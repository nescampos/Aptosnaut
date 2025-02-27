namespace Aptosnaut.Models
{
    public class AptosBalance
    {
        public string AssetType { get; set; }
        public decimal Amount { get; set; }
        public string? TokenStandard { get; set; }
        public bool IsFrozen { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public string? Icon { get; set; }
    }
}
