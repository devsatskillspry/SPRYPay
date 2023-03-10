namespace SPRYPayServer.Models.WalletViewModels
{
    public class WalletModel
    {
        public string ServerUrl { get; set; }
        public string CryptoCurrency
        {
            get;
            set;
        }
        public string DefaultAddress { get; set; }
        public string DefaultAmount { get; set; }

        public decimal? Rate { get; set; }
        public int Divisibility { get; set; }
        public string Fiat { get; set; }
        public string RateError { get; set; }
    }
}
