FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
WORKDIR /app
COPY . .

ARG app_version
ARG app_compilation_date
ENV APP_VERSION=${app_version}
ENV APP_COMPILATION_DATE=${app_compilation_date}

ENTRYPOINT ["dotnet", "SimpleTrading.Candles.HttpServer.dll"]
