using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleTrading.Telemetry;

namespace SimpleTrading.Candles.HttpServer.Middlewares
{
    public class ExceptionLogMiddleware
    {
        private readonly RequestDelegate _next;
 
        public ExceptionLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }
 
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                ex.WriteToActivity();
                ServiceLocator.Logger.Error(ex, ex.Message);
                
            }
        }

    }
}