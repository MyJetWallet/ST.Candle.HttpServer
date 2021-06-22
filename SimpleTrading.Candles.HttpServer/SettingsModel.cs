using SimpleTrading.SettingsReader;

namespace SimpleTrading.Candles.HttpServer
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("CandlesHttp.ServiceBusHostPort")]
        public string ServiceBusHostPort { get; set; }

        [YamlProperty("CandlesHttp.ExpiresMinutes")]
        public string ExpiresMinutes { get; set; }

        [YamlProperty("CandlesHttp.ExpiresHours")]
        public string ExpiresHours { get; set; }
        
        [YamlProperty("CandlesHttp.CandlesSource")]
        public string CandlesSource { get; set; }    
        
        [YamlProperty("CandlesHttp.JaegerUrl")]
        public string JaegerUrl { get; set; }        
        [YamlProperty("CandlesHttp.SeqUrl")]
        public string SeqUrl { get; set; }
    }
}