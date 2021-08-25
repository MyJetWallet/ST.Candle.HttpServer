using System;
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
using SimpleTrading.CandlesCache;

namespace SimpleTrading.Candles.HttpServer.Controllers
{
    [Route("api/v2/Candles")]
    public class CandlesV2Controller : ControllerBase
    {
        [HttpGet("Candles/{instrumentId}/{type}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IActionResult> Candles(
            [FromRoute] string instrumentId,
            [FromRoute] CandleType type,
            [FromQuery] [Required] CandlesHistoryV2Request requestContracts)
        {
            try
            {
                HttpContext.GetTraderId();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized("UnAuthorized request");
            }


            var result = ServiceLocator.CandlesHistoryCache.Get(instrumentId, type,
                requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                requestContracts.FromDate.UnixTimeToDateTime(),
                requestContracts.ToDate.UnixTimeToDateTime());

            if (requestContracts.MergeCandlesCount == 1)
                return Ok(result.Select(CandleApiModel.Create));

            var mergedCandles = result
                .SplitToChunks(requestContracts.MergeCandlesCount)
                .Select(itm => itm.Merge());

            return Ok(mergedCandles.Select(CandleApiModel.Create));
        }

        [HttpGet("LastCandles/{instrumentId}/{type}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IActionResult> LastCandles(
            [FromRoute] string instrumentId,
            [FromRoute] CandleType type,
            [FromQuery] [Required] LastCandlesV2Request requestContracts)
        {
            try
            {
                HttpContext.GetTraderId();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized("UnAuthorized request");
            }

            var result = ServiceLocator.CandlesHistoryCache.Get(instrumentId,
                type,
                requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                requestContracts.Amount * requestContracts.MergeCandlesCount);

            if (requestContracts.MergeCandlesCount == 0)
            {
                return Ok(result.Select(CandleApiModel.Create));
            }

            var mergedCandles = result
                .SplitToChunks(requestContracts.MergeCandlesCount)
                .Select(itm => itm.Merge());

            return Ok(mergedCandles.Select(CandleApiModel.Create));
        }
    }
}