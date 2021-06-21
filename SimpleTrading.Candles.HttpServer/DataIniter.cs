using System;
using SimpleTrading.CandlesCache;
using SimpleTrading.CandlesHistory.AzureStorage;

namespace SimpleTrading.Candles.HttpServer
{
    public static class DataIniter
    {
        public static void InitData(SettingsModel settingsModel)
        {
            var minuteExpirationDate = DateTime.UtcNow - TimeSpan.Parse(settingsModel.ExpiresMinutes);
            var hourExpirationDate = DateTime.UtcNow - TimeSpan.Parse(settingsModel.ExpiresHours);

            CandlesHistoryLoader.LoadCandlesAsync(
                CandlesPersistentAzureStorage,
                CandlesHistoryCache,
                minuteExpirationDate,
                hourExpirationDate,
                true
            ).Wait();

            CandlesHistoryLoader.LoadCandlesAsync(
                CandlesPersistentAzureStorage,
                CandlesHistoryCache,
                minuteExpirationDate,
                hourExpirationDate,
                false
            ).Wait();
        }
    }
}