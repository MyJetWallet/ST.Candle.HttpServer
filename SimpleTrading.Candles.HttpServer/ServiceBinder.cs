using System;
using DotNetCoreDecorators;
using Microsoft.Extensions.DependencyInjection;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using SimpleTrading.Abstraction.BidAsk;
using SimpleTrading.CandlesCache;
using SimpleTrading.ServiceBus.PublisherSubscriber.BidAsk;

namespace SimpleTrading.Candles.HttpServer
{
    public static class ServiceBinder
    {
        private const string EnvInfo = "ENV_INFO";
        private const string AppName = "CandlesHttp";
        private static string AppNameWithEnvMark => $"{AppName}-{GetEnvInfo()}";
        
        public static MyServiceBusTcpClient BindServiceBus(this IServiceCollection sr, SettingsModel settingsModel)
        {
            var tcpClient = new MyServiceBusTcpClient(() => settingsModel.ServiceBusHostPort, AppNameWithEnvMark);
            
            sr.AddSingleton<ISubscriber<IBidAsk>>(
                new BidAskMyServiceBusSubscriber(tcpClient, AppName, TopicQueueType.PermanentWithSingleConnection, false));
            
            return tcpClient;
        }
        
        public static MyServiceBusTcpClient BindCacheQueue(this IServiceCollection sr, SettingsModel settingsModel)
        {
            var tcpClient = new MyServiceBusTcpClient(() => settingsModel.ServiceBusHostPort, AppNameWithEnvMark);
            var bidAskSubscriber = new BidAskMyServiceBusSubscriber(tcpClient, AppName,
                TopicQueueType.DeleteOnDisconnect, false);
            
            var candlesQueue = new SaveCandleQueue();
            var candlesHistoryCache = new CandlesHistoryCache(candlesQueue);
            
            sr.AddSingleton<ISubscriber<IBidAsk>>(bidAskSubscriber);
            sr.AddSingleton<ISaveCandleQueue>(candlesQueue);
            sr.AddSingleton<ICandlesHistoryCache>(candlesHistoryCache);

            return tcpClient;
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