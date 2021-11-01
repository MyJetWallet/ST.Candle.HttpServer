using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SimpleTrading.Candles.HttpServer.Controllers;
using SimpleTrading.Candles.HttpServer.Models;

namespace ST.Candle.HttpServer.Tests
{
    public class NormalizeCandlesTester
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var list1 = new List<CandleApiModel>()
            {
                new() {D = 1},
                new() {D = 2},
                new() {D = 3}
            };
            var list2 = new List<CandleApiModel>()
            {
                new() {D = 1},
                new() {D = 2},
                new() {D = 3}
            };
            
            var list1Dates = NormalizeAndWriteLine(list1, list2, out var list2Dates);

            Assert.AreEqual(list1Dates, list2Dates);
        }
        
        [Test]
        public void Test2()
        {
            var list1 = new List<CandleApiModel>()
            {
                new() {D = 1},
                new() {D = 3}
            };
            var list2 = new List<CandleApiModel>()
            {
                new() {D = 1},
                new() {D = 2},
                new() {D = 3}
            };
            
            var list1Dates = NormalizeAndWriteLine(list1, list2, out var list2Dates);

            Assert.AreEqual(list1Dates, list2Dates);
        }
        [Test]
        public void Test3()
        {
            var list1 = new List<CandleApiModel>()
            {
                new() {D = 1},
                new() {D = 2},
                new() {D = 3}
            };
            var list2 = new List<CandleApiModel>()
            {
                new() {D = 2},
                new() {D = 3}
            };
            
            var list1Dates = NormalizeAndWriteLine(list1, list2, out var list2Dates);

            Assert.AreEqual(list1Dates, list2Dates);
        }
        [Test]
        public void Test4()
        {
            var list1 = new List<CandleApiModel>()
            {
                new() {D = 2},
                new() {D = 3}
            };
            var list2 = new List<CandleApiModel>()
            {
                new() {D = 1},
                new() {D = 2}
            };
            
            var list1Dates = NormalizeAndWriteLine(list1, list2, out var list2Dates);

            Assert.AreEqual(list1Dates, list2Dates);
        }
        
        [Test]
        public void Test5()
        {
            var list1 = new List<CandleApiModel>()
            {
                new() {D = 1},
                new() {D = 2},
                new() {D = 3}
            };
            var list2 = new List<CandleApiModel>()
            {
                new() {D = 4},
                new() {D = 5},
                new() {D = 6}
            };
            
            var list1Dates = NormalizeAndWriteLine(list1, list2, out var list2Dates);

            Assert.AreEqual(list1Dates, list2Dates);
        }

        private static List<long> NormalizeAndWriteLine(List<CandleApiModel> list1, List<CandleApiModel> list2, out List<long> list2Dates)
        {
            Console.WriteLine("---------------------------BEFORE NORMALIZING---------------------------");
            Console.WriteLine("---------------------------List 1 dates---------------------------");
            var list1Dates = list1.Select(candleApiModel => candleApiModel.D).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(list1Dates));
            Console.WriteLine("---------------------------List 2 dates---------------------------");
            list2Dates = list2.Select(candleApiModel => candleApiModel.D).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(list2Dates));

            (list1, list2) = CandlesV3Controller.NormalizeCandles(list1, list2);

            Console.WriteLine("---------------------------AFTER NORMALIZING---------------------------");
            Console.WriteLine("---------------------------List 1 dates---------------------------");
            list1Dates = list1.Select(candleApiModel => candleApiModel.D).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(list1Dates));
            Console.WriteLine("---------------------------List 2 dates---------------------------");
            list2Dates = list2.Select(candleApiModel => candleApiModel.D).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(list2Dates));
            return list1Dates;
        }
    }
}