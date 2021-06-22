using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SimpleTrading.Abstraction.Candles;
using SimpleTrading.Candles.HttpServer.Models;
using SimpleTrading.Candles.HttpServer.Models.RequestResponse;
using SimpleTrading.CandlesHistory.Grpc.Contracts;

namespace SimpleTrading.Candles.HttpServer.Controllers
{
    [Route("api/v2/Candles")]
    public class CandlesV2Controller : ControllerBase
    {
        [HttpGet("Candles/{source}/{instrumentId}/{type}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IEnumerable<CandleApiModel>> Candles(
            [FromRoute] string source,
            [FromRoute] string instrumentId,
            [FromRoute] CandleType type,
            [FromQuery] [Required] CandlesHistoryV2Request requestContracts)
        {
            HttpContext.GetTraderId();

            var grpcRequest = new GetCandlesHistoryGrpcRequestContract
            {
                Instrument = instrumentId,
                From = requestContracts.FromDate.UnixTimeToDateTime(),
                To = requestContracts.ToDate.UnixTimeToDateTime(),
                Bid = requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                CandleType = type.ToGrpc()
            };

            var response = (await ServiceLocator.CandlesHistoryGrpc.GetCandlesHistoryAsync(grpcRequest)).ToList();
            return response.Select(itm => itm.ToRest());
        }

        [HttpGet("LastCandles/{source}/{instrumentId}/{type}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IEnumerable<CandleApiModel>> LastCandles(
            [FromRoute] string source,
            [FromRoute] string instrumentId,
            [FromRoute] CandleType type,
            [FromQuery] [Required] LastCandlesV2Request requestContracts)
        {
            HttpContext.GetTraderId();

            var grpcRequest = new GetLastCandlesGrpcRequestContract
            {
                Instrument = instrumentId,
                Amount = requestContracts.Amount,
                Bid = requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                CandleType = type.ToGrpc()
            };

            var response = (await ServiceLocator.CandlesHistoryGrpc.GetLastCandlesAsync(grpcRequest)).ToList();
            return response.Select(itm => itm.ToRest());
        }
    }
}