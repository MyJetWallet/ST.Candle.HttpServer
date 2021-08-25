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
        public static CandleType ToDomain(this CandleTypeGrpcModel src)
        {
            return src switch
            {
                CandleTypeGrpcModel.Minute => CandleType.Minute,
                CandleTypeGrpcModel.Hour => CandleType.Hour,
                CandleTypeGrpcModel.Day => CandleType.Day,
                CandleTypeGrpcModel.Month => CandleType.Month,
                _ => throw new ArgumentOutOfRangeException(nameof(src), src, null)
            };
        }
        
        public static CandleTypeGrpcModel ToGrpc(this CandleType src)
        {
            return src switch
            {
                CandleType.Minute => CandleTypeGrpcModel.Minute,
                CandleType.Hour => CandleTypeGrpcModel.Hour,
                CandleType.Day => CandleTypeGrpcModel.Day,
                CandleType.Month => CandleTypeGrpcModel.Month,
                _ => throw new ArgumentOutOfRangeException(nameof(src), src, null)
            };
        }

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