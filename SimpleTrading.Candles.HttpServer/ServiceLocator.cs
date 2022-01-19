using System;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyDependencies;
using Serilog;
using SimpleTrading.Abstraction.BidAsk;
using SimpleTrading.Abstraction.Candles;
using SimpleTrading.CandlesCache;
using SimpleTrading.CandlesHistory.Grpc;
using SimpleTrading.CandlesHistory.Grpc.Contracts;
using SimpleTrading.ServiceBus.Contracts;
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
        
        public static ISubscriber<UpdateCandlesHistoryServiceBusContract> UpdateCandlesSubscriber { get; private set; }
        
        public static ISubscriber<CandleMigrationServiceBusContract> MigrationCandlesSubscriber { get; private set; }

        public static ILogger Logger { get; private set; }

        public static void BindSubscribers()
        {
            BidAskSubscriber.Subscribe(itm =>
            {
                CandlesHistoryCache.NewBidAsk(itm.Id, new[] { itm });
                return new ValueTask();
            });

            UpdateCandlesSubscriber.Subscribe(async updateEvent =>
            {
                if (updateEvent.CacheIsUpdated)
                {
                    Logger.Information(
                        "CandlesHttpServer - Updating candles cache after upload: {id}[type-{type}] candles for {dateFrom}-{dateTo} get started",
                        updateEvent.InstrumentId,
                        updateEvent.CandleType,
                        updateEvent.DateFrom,
                        updateEvent.DateTo);

                    await UpdateCandles(updateEvent, true);

                    Logger.Information(
                       "CandlesHttpServer - Updating candles cache after upload: {id}[type-{type}] candles for {dateFrom}-{dateTo} get finished",
                       updateEvent.InstrumentId,
                       updateEvent.CandleType,
                       updateEvent.DateFrom,
                       updateEvent.DateTo);
                }
            });
        }

        public static async Task InitData()
        {
            using (TelemetryExtensions.StartActivity("load-bids-to-cache"))
            {
                Console.WriteLine("load-bids-to-cache");
                await InitBidAskToCandles(true);
            }

            using (TelemetryExtensions.StartActivity("load-asks-to-cache"))
            {
                Console.WriteLine("load-asks-to-cache");
                await InitBidAskToCandles(false);
            }
            
            Console.WriteLine("Done candles load");
        }

        public static void Init(IServiceResolver sr, string sessionEncodingKey)
        {
            SessionEncodingKey = Encoding.UTF8.GetBytes(sessionEncodingKey);
            CandlesHistoryGrpc = sr.GetService<ISimpleTradingCandlesHistoryGrpc>();
            CandlesHistoryCache = sr.GetService<ICandlesHistoryCache>();
            BidAskSubscriber = sr.GetService<ISubscriber<IBidAsk>>();
            UpdateCandlesSubscriber = sr.GetService<ISubscriber<UpdateCandlesHistoryServiceBusContract>>();
            MigrationCandlesSubscriber = sr.GetService<ISubscriber<CandleMigrationServiceBusContract>>();

            Logger = sr.GetService<ILogger>();
            InitData().Wait();
            
            MigrationCandlesSubscriber.Subscribe(async candle => CandlesHistoryCache.UpdateCandle(candle.Symbol, candle.Candle, candle.IsBid, candle.Data));
            
            MigrationCandlesSubscriber.Subscribe(async candle =>
            {
                var minuteExpirationDate = DateTime.UtcNow - TimeSpan.Parse(SettingsModel.ExpiresMinutes);
                var hourExpirationDate = DateTime.UtcNow - TimeSpan.Parse(SettingsModel.ExpiresHours);

                switch (candle.Candle)
                {
                    case CandleType.Minute:
                    {
                        if (candle.Data.DateTime > minuteExpirationDate)
                            CandlesHistoryCache.UpdateCandle(candle.Symbol, candle.Candle, candle.IsBid, candle.Data);
                        break;
                    }
                    case CandleType.Hour:
                    {
                        if (candle.Data.DateTime > hourExpirationDate)
                            CandlesHistoryCache.UpdateCandle(candle.Symbol, candle.Candle, candle.IsBid, candle.Data);
                        break;
                    }
                    case CandleType.Day:
                    case CandleType.Month:
                    default:
                        CandlesHistoryCache.UpdateCandle(candle.Symbol, candle.Candle, candle.IsBid, candle.Data);
                        break;
                }
            });

        }

        private static async Task InitBidAskToCandles(bool isBids)
        {
            var count = 0;
            await foreach (var itm in CandlesHistoryGrpc.GetAllFromCacheAsync(new GetAllFromCacheGrpcRequest
            {
                IsBids = isBids,
            }))
            {
                if (count % 5000 == 0)
                {
                    Console.WriteLine($"Loading bid-ask. Count: {count}");
                }
                CandlesHistoryCache.Init(
                    itm.InstrumentId,
                    isBids,
                    itm.CandleType,
                    itm.Candle.ToDomain());
                count++;
            }
        }

        private static async Task UpdateCandles(UpdateCandlesHistoryServiceBusContract updateEvent, bool isBids)
        {
            var count = 0;
            await foreach (var itm in CandlesHistoryGrpc.GetCandlesHistoryStream(
                new GetCandlesHistoryGrpcRequestContract
                {
                    Instrument = updateEvent.InstrumentId,
                    CandleType = updateEvent.CandleType,
                    Bid = isBids,
                    From = updateEvent.DateFrom,
                    To = updateEvent.DateTo,
                    Source = "ST"
                }))
            {
                if (count % 5000 == 0)
                {
                    Console.WriteLine($"Loading bid-ask. Count: {count}");
                }
                CandlesHistoryCache.Init(
                    updateEvent.InstrumentId,
                    isBids,
                    updateEvent.CandleType,
                    itm.ToDomain());
                count++;
            }
        }
    }
}