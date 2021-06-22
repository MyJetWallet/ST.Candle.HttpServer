using System.Text;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyDependencies;
using Serilog;
using SimpleTrading.Abstraction.BidAsk;
using SimpleTrading.CandlesCache;
using SimpleTrading.CandlesHistory.Grpc;
using SimpleTrading.CandlesHistory.Grpc.Contracts;
using SimpleTrading.Telemetry;

namespace SimpleTrading.Candles.HttpServer
{
    public static class ServiceLocator
    {
        public static ISimpleTradingCandlesHistoryGrpc CandlesHistoryGrpc { get; set; }
        public static SettingsModel SettingsModel { get; set; }
        public static byte[] SessionEncodingKey { get; private set; }
        public static ICandlesHistoryCache CandlesHistoryCache { get; private set; }
        public static ISubscriber<IBidAsk> BidAskSubscriber { get; private set; }
        public static ILogger Logger { get; private set; }

        public static void BindSubscribers()
        {
            BidAskSubscriber.Subscribe(itm =>
            {
                CandlesHistoryCache.NewBidAsk(itm.Id, new[] { itm });
                return new ValueTask();
            });
        }

        public static async Task InitData()
        {
            using (TelemetryExtensions.StartActivity("load-bids-to-cache"))
            {
                await InitBidAskToCandles(true);
            }

            using (TelemetryExtensions.StartActivity("load-asks-to-cache"))
            {
                await InitBidAskToCandles(false);
            }
        }

        public static void Init(IServiceResolver sr, string sessionEncodingKey)
        {
            SessionEncodingKey = Encoding.UTF8.GetBytes(sessionEncodingKey);
            CandlesHistoryGrpc = sr.GetService<ISimpleTradingCandlesHistoryGrpc>();
            CandlesHistoryCache = sr.GetService<ICandlesHistoryCache>();
            BidAskSubscriber = sr.GetService<ISubscriber<IBidAsk>>();
            Logger = sr.GetService<ILogger>();
            InitData().Wait();
        }

        private static async Task InitBidAskToCandles(bool isBids)
        {
            await foreach (var itm in CandlesHistoryGrpc.GetAllFromCacheAsync(new GetAllFromCacheGrpcRequest
            {
                IsBids = isBids,
            }))
            {
                CandlesHistoryCache.Init(itm.InstrumentId, isBids, itm.CandleType.ToDomain(), itm.Candle.ToDomain());
            }
        }
    }
}