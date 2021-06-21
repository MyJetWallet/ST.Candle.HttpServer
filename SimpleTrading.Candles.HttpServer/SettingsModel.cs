namespace SimpleTrading.Candles.HttpServer
{
    public class SettingsModel
    {
        public string ServiceBusHostPort { get; set; }
        
        public string ExpiresMinutes { get; set; }
        
        public string ExpiresHours { get; set; }
    }
}