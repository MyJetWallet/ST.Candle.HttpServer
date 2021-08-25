using System;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using SimpleTrading.Abstraction.Candles;
using SimpleTrading.Telemetry;

namespace SimpleTrading.Candles.HttpServer
{
    public class BackgroundJobs
    {
        private static readonly TaskTimer TaskTimerMinute = new TaskTimer(TimeSpan.Parse("00:01:00"));
        private static readonly TaskTimer TaskTimerHour = new TaskTimer(TimeSpan.Parse("01:00:00"));
        
        public static void Init()
        {
            var minuteTimeSpan = TimeSpan.Parse(ServiceLocator.SettingsModel.ExpiresMinutes);
            var hourTimeSpan = TimeSpan.Parse(ServiceLocator.SettingsModel.ExpiresHours);
            
            TaskTimerMinute.Register("minute candles cleaner", () =>
            {
                TelemetryExtensions.StartActivity("minute-candles-garbage-collect");
                
                if (minuteTimeSpan.Equals(TimeSpan.Zero))
                    return new ValueTask();

                var dt = DateTime.UtcNow - minuteTimeSpan;
                
                ServiceLocator.CandlesHistoryCache.CleanCandles(CandleType.Minute, true, dt);
                ServiceLocator.CandlesHistoryCache.CleanCandles(CandleType.Minute, false, dt);
                
                return new ValueTask();
            });

            TaskTimerHour.Register("hour candles cleaner", () =>
            {
                TelemetryExtensions.StartActivity("hour-candles-garbage-collect");
                if (hourTimeSpan.Equals(TimeSpan.Zero))
                    return new ValueTask();
                
                var dt = DateTime.UtcNow - hourTimeSpan;
                
                ServiceLocator.CandlesHistoryCache.CleanCandles(CandleType.Hour, true, dt);
                ServiceLocator.CandlesHistoryCache.CleanCandles(CandleType.Hour, false, dt);
                
                return new ValueTask();
            });
        }

        public static void Start()
        {
            TaskTimerMinute.Start();
            TaskTimerHour.Start();
        }
    }
}