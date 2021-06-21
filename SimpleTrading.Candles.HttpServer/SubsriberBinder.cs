using System.Threading.Tasks;
using DotNetCoreDecorators;
using SimpleTrading.Abstraction.BidAsk;
using SimpleTrading.CandlesCache;

namespace SimpleTrading.Candles.HttpServer
{
    public static class SubscriberBinder
    {
        public static void Init(ISubscriber<IBidAsk> bidAskSubscriber, ICandlesHistoryCache cache)
        {
            bidAskSubscriber.Subscribe(itm =>
            {
                cache.NewBidAsk(itm.Id, new[]{itm});
                return new ValueTask();
            });
        }
    }
}