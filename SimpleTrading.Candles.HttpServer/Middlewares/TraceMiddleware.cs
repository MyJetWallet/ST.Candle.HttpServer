using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleTrading.Telemetry;
using SimpleTrading.TokensManager.Tokens;

namespace SimpleTrading.Candles.HttpServer.Middlewares
{
#pragma warning disable
    public class TraceMiddleware
    {
        private readonly RequestDelegate _next;
        private const string AuthorizationHeader = "authorization";

        public TraceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey(AuthorizationHeader))
            {
                var itm = context.Request.Headers[AuthorizationHeader].ToString().Trim();
                var items = itm.Split();
                var (_, token) = TokensManager.TokensManager.ParseBase64Token<AuthorizationToken>(items[^1],
                    ServiceLocator.SessionEncodingKey, DateTime.UtcNow);

                token.Id.AddToActivityAndChildren("traderId");
                token.Id.AddToActivityAsTag("traderId");
            }

            await _next(context);
        }
    }
}