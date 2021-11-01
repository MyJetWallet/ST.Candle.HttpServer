using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimpleTrading.Candles.HttpServer.Extensions
{
    public static class ListExtensions
    {
        public static bool TryGetValue<T>(this List<T> obj, int index, 
            [MaybeNullWhen(false)] out T value)
        {
            if (obj.Count > index)
            {
                value = obj[index];
                return true;
            }
            value = default;
            return false;
        }
    }
}