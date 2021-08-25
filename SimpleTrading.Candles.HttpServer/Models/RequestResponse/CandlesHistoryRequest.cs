using System.ComponentModel.DataAnnotations;
using SimpleTrading.Abstraction.Candles;

namespace SimpleTrading.Candles.HttpServer.Models.RequestResponse
{
    public enum CandlesContractBidOrAsk
    {
        Bid,
        Ask
    }

    public class CandlesHistoryRequest
    {
        [Required]
        public string InstrumentId { get; set; }
        
        public CandlesContractBidOrAsk BidOrAsk { get; set; }
        
        public long FromDate { get; set; }
        
        public long ToDate { get; set; }
        
        public CandleType CandleType { get; set; }
    }
    
    public class CandlesHistoryV2Request
    {
        public CandlesContractBidOrAsk BidOrAsk { get; set; }
        public long FromDate { get; set; }
        public long ToDate { get; set; }
        public int MergeCandlesCount { get; set; } = 1;
    }
}