using System;
using DotNetCoreDecorators;
using SimpleTrading.Abstraction.Candles;
using SimpleTrading.Candles.HttpServer.Models;
using SimpleTrading.CandlesCache;
using SimpleTrading.CandlesHistory.Grpc.Models;

namespace SimpleTrading.Candles.HttpServer
{
    public static class ModelsUtils
    {
        public static CandleApiModel ToRest(this CandleGrpcModel src)
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
        
        public static ICandleModel ToDomain(this CandleGrpcModel src)
        {
            return new CandleModel
            {
                DateTime = src.DateTime,
                Close = src.Close,
                High = src.High,
                Low = src.Low,
                Open = src.Open
            };
        }
    }
}