using System;
using Microsoft.AspNetCore.Http;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;

namespace SimpleTrading.Candles.HttpServer
{
    public static class RequestUtils
    {
        private const string AuthorizationHeader = "authorization";
        
        public static string GetTraderId(this HttpContext ctx)
        {
            if (!ctx.Request.Headers.ContainsKey(AuthorizationHeader))
                throw new UnauthorizedAccessException("UnAuthorized request");
            
            var itm = ctx.Request.Headers[AuthorizationHeader].ToString().Trim();
            var items = itm.Split();
            return items[^1].GetTraderIdByToken();
        }
        
        public static string GetTraderIdByToken(this string tokenString)
        {
            try
            {
                var (result ,token) = TokensManager.TokensManager.ParseBase64Token<AuthorizationToken>(tokenString, ServiceLocator.SessionEncodingKey, DateTime.UtcNow);

                return result switch
                {
                    TokenParseResult.Expired => throw new UnauthorizedAccessException("UnAuthorized request"),
                    TokenParseResult.InvalidToken => throw new UnauthorizedAccessException("UnAuthorized request"),
                    _ => token.Id
                };
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException("UnAuthorized request");
            }
        }

    }
}