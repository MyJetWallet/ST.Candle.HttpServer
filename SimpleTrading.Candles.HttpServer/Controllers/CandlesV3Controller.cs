using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSwag.Annotations;
using SimpleTrading.Abstraction.Candles;
using SimpleTrading.Candles.HttpServer.Extensions;
using SimpleTrading.Candles.HttpServer.Models;
using SimpleTrading.Candles.HttpServer.Models.RequestResponse;
using SimpleTrading.CandlesCache;

namespace SimpleTrading.Candles.HttpServer.Controllers
{
    [Route("api/v3/Candles")]
    public class CandlesV3Controller : ControllerBase
    {
        [HttpGet("Candles/{type}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(List<CandleApiModel>), Description = "Ok")]
        public async Task<IActionResult> Candles(
            [FromRoute] CandleType type,
            [FromQuery] [Required] CandlesHistoryV3Request requestContracts)
        {
            try
            {
                HttpContext.GetTraderId();
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized("UnAuthorized request");
            }

            var instructions = new List<Instruction>();

            try
            {
                var subs = requestContracts.Instruction.Split(";");

                foreach (var sub in subs)
                {
                    var subsBySub = sub.Split(":");

                    if (subsBySub.Length == 1)
                    {
                        return Ok(GetCandles(requestContracts.Instruction, type, requestContracts));
                    }

                    var symbol = subsBySub[0];
                    var isDirect = subsBySub[1];

                    instructions.Add(new Instruction()
                    {
                        InstrumentSymbol = symbol,
                        IsDirect = isDirect == "1"
                    });
                }
            }
            catch (Exception)
            {
                return BadRequest("Bad instruction.");
            }

            var resultCandles = new List<CandleApiModel>();

            foreach (var instructionEntity in instructions)
            {
                var candles = GetCandles(instructionEntity.InstrumentSymbol, type, requestContracts).ToList();

                if (!resultCandles.Any() && candles.Any())
                {
                    if (instructionEntity.IsDirect)
                    {
                        resultCandles = candles;
                    }
                    else
                    {
                        resultCandles  = GetReversedCandles(candles);
                    } 
                    continue;
                }

                if (resultCandles.Any())
                {
                    if (resultCandles.Count != candles.Count)
                    {
                        (resultCandles, candles) = NormalizeCandles(resultCandles, candles);
                    }
                    else
                    {
                        resultCandles = resultCandles.OrderBy(e => e.D).ToList();
                        candles = candles.OrderBy(e => e.D).ToList();
                    }

                    if (!candles.Any())
                    {
                        ServiceLocator.Logger.Error($"Candles for {instructionEntity.InstrumentSymbol} not found. Query: {JsonConvert.SerializeObject(requestContracts)}");
                        return Ok(new List<CandleApiModel>());
                    }
                    for (var i = 0; i < resultCandles.Count; i++)
                    {
                        var resCandle = resultCandles[i];
                        var candle = candles[i];

                        if (resCandle.D == candle.D)
                        {
                            MergeCandles(resCandle, candle, instructionEntity.IsDirect);
                        }
                        if (resCandle.D < candle.D)
                        {
                            var weDidIt = false;
                            for (var j = i; j > 0; j--)
                            {
                                candle = candles[j];
                                if (candle.D <= resCandle.D)
                                {
                                    weDidIt = true;
                                    MergeCandles(resCandle, candle, instructionEntity.IsDirect);
                                    break;
                                }
                            }
                            if (!weDidIt)
                            {
                                candle = candles.First();
                                MergeCandles(resCandle, candle, instructionEntity.IsDirect, true);
                            }
                        }
                    }
                }
            }
            return Ok(resultCandles);
        }

        public static (List<CandleApiModel>, List<CandleApiModel>) NormalizeCandles(List<CandleApiModel> list1, List<CandleApiModel> list2)
        {
            var maxIterationNumber = list1.Count + list2.Count + 1;
            var iteration = 0;

            list1 = list1.OrderBy(e => e.D).ToList();
            list2 = list2.OrderBy(e => e.D).ToList();
            
            while (iteration < maxIterationNumber)
            {
                CandleApiModel list1Elem = null;
                CandleApiModel list2Elem = null;
                if (list1.TryGetValue(iteration, out var result1))
                    list1Elem = result1;
                
                if (list2.TryGetValue(iteration, out var result2))
                    list2Elem = result2;
                if (list1Elem == null && list2Elem == null)
                {
                    break;
                }
                else if (list1Elem == null)
                {
                    list2.Remove(list2Elem);
                } 
                else if (list2Elem == null)
                {
                    list1.Remove(list1Elem);
                }
                else if (list1Elem.D < list2Elem.D)
                {
                    list1.Remove(list1Elem);
                } 
                else if (list2Elem.D < list1Elem.D)
                {
                    list2.Remove(list2Elem);
                }
                else if (list1Elem.D == list2Elem.D)
                {
                    iteration ++;
                }
                else
                {
                    Console.WriteLine($"ERROR. Find straight scenario in NormalizeCandles.");
                }
            }
            return (list1, list2);
        }

        private void MergeCandles(CandleApiModel resCandle, CandleApiModel candle, bool isDirect, bool useOpen = false)
        {
            var koef = useOpen ? candle.O : candle.C;
            if (isDirect)
            {
                resCandle.O = Math.Round(resCandle.O * koef, 8);
                resCandle.C = Math.Round(resCandle.C * koef, 8);
                resCandle.H = Math.Round(resCandle.H * koef, 8);
                resCandle.L = Math.Round(resCandle.L * koef, 8);
            }
            else
            {
                resCandle.O = Math.Round(resCandle.O / koef, 8);
                resCandle.C = Math.Round(resCandle.C / koef, 8);
                resCandle.H = Math.Round(resCandle.H / koef, 8);
                resCandle.L = Math.Round(resCandle.L / koef, 8);
            }
        }

        private List<CandleApiModel> GetReversedCandles(IEnumerable<CandleApiModel> candleApiModels)
        {
            var reversedCandles = candleApiModels.Select(e => new CandleApiModel()
            {
                D = e.D,
                C = 1 / e.C,
                O = 1 / e.O,
                H = 1 / e.L,
                L = 1 / e.H
            }).ToList();
            return reversedCandles;
        }

        private IEnumerable<CandleApiModel> GetCandles(string instrumentSymbol, CandleType type, CandlesHistoryV3Request requestContracts)
        {
            var result = ServiceLocator.CandlesHistoryCache.Get(instrumentSymbol, type,
                requestContracts.BidOrAsk == CandlesContractBidOrAsk.Bid,
                requestContracts.FromDate.UnixTimeToDateTime(),
                requestContracts.ToDate.UnixTimeToDateTime());
            
            if (requestContracts.MergeCandlesCount == 1)
                return result.Select(CandleApiModel.Create);

            var mergedCandles = result
                .SplitToChunks(requestContracts.MergeCandlesCount)
                .Select(itm => itm.Merge());

            return mergedCandles.Select(CandleApiModel.Create);
        }
    }

    public class Instruction
    {
        public string InstrumentSymbol { get; set; }
        public bool IsDirect { get; set; }
    }
}