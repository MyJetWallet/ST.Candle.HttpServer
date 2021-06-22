using DotNetCoreDecorators;
using SimpleTrading.Abstraction.Candles;

namespace SimpleTrading.Candles.HttpServer.Models
{
    public class CandleApiModel
    {
        public long D { get; set; }
        public double O { get; set; }
        public double C { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        
        public static CandleApiModel Create(ICandleModel src)
        {
            return new CandleApiModel
            {
                D = src.DateTime.UnixTime(),
                C = src.Close,
                H = src.High,
                L = src.Low,
                O = src.Open
            };
        }
    }
}