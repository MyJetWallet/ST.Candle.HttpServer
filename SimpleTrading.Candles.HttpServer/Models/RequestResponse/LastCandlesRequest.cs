using SimpleTrading.Abstraction.Candles;

namespace SimpleTrading.Candles.HttpServer.Models.RequestResponse
{
    public class LastCandlesRequest
    {
        public string InstrumentId { get; set; }
        public CandlesContractBidOrAsk BidOrAsk { get; set; }
        public int Amount { get; set; }
        public CandleType CandleType { get; set; }
    }
    
    public class LastCandlesV2Request
    {
        public CandlesContractBidOrAsk BidOrAsk { get; set; }
        public int Amount { get; set; }
        
        public int MergeCandlesCount { get; set; } = 1;
    }
}