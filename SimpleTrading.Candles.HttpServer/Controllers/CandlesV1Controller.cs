using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SimpleTrading.Candles.HttpServer.Models;
using SimpleTrading.Candles.HttpServer.Models.RequestResponse;
using SimpleTrading.CandlesHistory.Grpc.Contracts;

namespace SimpleTrading.Candles.HttpServer.Controllers
{
    [Route("api/v1/PriceHistory")]
    public class CandlesV1Controller : ControllerBase
    {
        [HttpGet("Candles")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IEnumerable<CandleApiModel>> Candles(
            [FromQuery] [Required] CandlesHistoryRequest requestContracts)
        {
            HttpContext.GetTraderId();

            var grpcRequest = new GetCandlesHistoryGrpcRequestContract
            {
                Instrument = requestContracts.InstrumentId,
                From = requestContracts.FromDate.UnixTimeToDateTime(),
                To = requestContracts.ToDate.UnixTimeToDateTime(),
                Bid = requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                CandleType = requestContracts.CandleType.ToGrpc()
            };

            var response = (await ServiceLocator.CandlesHistoryGrpc.GetCandlesHistoryAsync(grpcRequest)).ToList();
            return response.Select(itm => itm.ToRest());
        }

        /// <summary>
        /// Get Last Candles
        /// </summary>
        /// <param name="requestContracts">Request contract</param>
        /// <returns></returns>
        [HttpGet("LastCandles")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IEnumerable<CandleApiModel>> LastCandles(
            [FromQuery] [Required] LastCandlesRequest requestContracts)
        {
            HttpContext.GetTraderId();

            var grpcRequest = new GetLastCandlesGrpcRequestContract
            {
                Instrument = requestContracts.InstrumentId,
                Amount = requestContracts.Amount,
                Bid = requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                CandleType = requestContracts.CandleType.ToGrpc()
            };

            var response = (await ServiceLocator.CandlesHistoryGrpc.GetLastCandlesAsync(grpcRequest)).ToList();
            return response.Select(itm => itm.ToRest());
        }
    }
}