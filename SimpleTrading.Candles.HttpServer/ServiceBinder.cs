using System;
using DotNetCoreDecorators;
using MyDependencies;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Serilog;
using Serilog.Core;
using SimpleTrading.Abstraction.BidAsk;
using SimpleTrading.CandlesCache;
using SimpleTrading.ServiceBus.PublisherSubscriber.BidAsk;
using SimpleTrading.Telemetry;

namespace SimpleTrading.Candles.HttpServer
{
    public static class ServiceBinder
    {
        private const string EnvInfo = "ENV_INFO";
        private const string AppName = "CandlesHttp";
        private static string AppNameWithEnvMark => $"{AppName}-{GetEnvInfo()}";
        
        public static MyServiceBusTcpClient BindServiceBus(this IServiceRegistrator sr, SettingsModel settingsModel)
        {
            var tcpClient = new MyServiceBusTcpClient(() => settingsModel.ServiceBusHostPort, AppNameWithEnvMark);
            
            sr.Register<ISubscriber<IBidAsk>>(new BidAskMyServiceBusSubscriber(tcpClient, AppName, TopicQueueType.PermanentWithSingleConnection, false));

            return tcpClient;
        }
        
        public static void BindCacheQueue(this IServiceRegistrator sr, SettingsModel settingsModel)
        {
            var candlesQueue = new SaveCandleQueue();
            var cache = new CandlesHistoryCache(candlesQueue);
            
            sr.Register<ISaveCandleQueue>(candlesQueue);
            sr.Register<ICandlesHistoryCache>(cache);

        }
        
        public static Logger BindLogger(this IServiceRegistrator sr, SettingsModel settingsModel)
        {
            var logger = LogsUtils.ConfigurateLogger(AppName, settingsModel.SeqUrl);
            sr.Register<ILogger>(logger);

            return logger;

        }
        
        private static string GetEnvInfo()
        {
            var info = Environment.GetEnvironmentVariable(EnvInfo);
            if (string.IsNullOrEmpty(info))
                throw new Exception($"Env Variable {EnvInfo} is not found");

            return info;
        }
    }
}