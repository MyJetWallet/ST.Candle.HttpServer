using System;
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

namespace SimpleTrading.Candles.HttpServer.Controllers
{
    [Route("api/v1/PriceHistory")]
    public class CandlesV1Controller : ControllerBase
    {
        [HttpGet("Candles")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IActionResult> Candles(
            [FromQuery] [Required] CandlesHistoryRequest requestContracts)
        {
            try
            {
                HttpContext.GetTraderId();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized("UnAuthorized request");
            }

            var result = ServiceLocator.CandlesHistoryCache.Get(requestContracts.InstrumentId,
                requestContracts.CandleType,
                requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                requestContracts.FromDate.UnixTimeToDateTime(),
                requestContracts.ToDate.UnixTimeToDateTime());

            return Ok(result.Select(CandleApiModel.Create));
        }

        /// <summary>
        /// Get Last Candles
        /// </summary>
        /// <param name="requestContracts">Request contract</param>
        /// <returns></returns>
        [HttpGet("LastCandles")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IActionResult> LastCandles(
            [FromQuery] [Required] LastCandlesRequest requestContracts)
        {
            try
            {
                HttpContext.GetTraderId();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized("UnAuthorized request");
            }

            var result = ServiceLocator.CandlesHistoryCache.Get(requestContracts.InstrumentId,
                requestContracts.CandleType,
                requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                requestContracts.Amount);

            return Ok(result.Select(CandleApiModel.Create));
        }
    }
}