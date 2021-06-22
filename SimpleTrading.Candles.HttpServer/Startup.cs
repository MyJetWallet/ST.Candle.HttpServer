using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDependencies;
using MyServiceBus.TcpClient;
using Prometheus;
using SimpleTrading.BaseMetrics;
using SimpleTrading.Candles.HttpServer.Middlewares;
using SimpleTrading.ServiceStatusReporterConnector;
using SimpleTrading.Telemetry;

namespace SimpleTrading.Candles.HttpServer
{
    public class Startup
    {
        private const string SessionEncodingKeyEnv = "SESSION_ENCODING_KEY";
        private static readonly MyIoc Di = new ();
        private static readonly SettingsModel Settings = SettingsReader.SettingsReader.ReadSettings<SettingsModel>();
        public IConfiguration Configuration { get; }
        
        private MyServiceBusTcpClient _myServiceBusTcpClient;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            ServiceLocator.SettingsModel = Settings;
            services.BindTelemetry("CandlesHttp", "ST-", Settings.JaegerUrl);
            services.AddControllers();
            services.AddSwaggerDocument(o =>
            {
                o.Title = "SimpleTrading Candles API";
                o.GenerateEnumMappingDescription = true;
            });
            _myServiceBusTcpClient = Di.BindServiceBus(Settings);
            Di.BindCacheQueue(Settings);
            Di.BindLogger(Settings);
            Di.BindGrpcServices();
            BackgroundJobs.Init();
            ServiceLocator.Init(Di, GetSessionEncodingKey());
            
        }

        public async Task Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.BindServicesTree(Assembly.GetExecutingAssembly());
            app.BindMetricsMiddleware();
            app.BindIsAlive();
            
            app.UseMiddleware<TraceMiddleware>();
            app.UseMiddleware<ExceptionLogMiddleware>();
            
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
            await Start();
        }

        private async Task Start()
        {
            TelemetryExtensions.StartActivity("application-start");
            {
                await ServiceLocator.InitData();
                BackgroundJobs.Start();
                ServiceLocator.BindSubscribers();
                _myServiceBusTcpClient.Start();
            }
        }
        
        private static string GetSessionEncodingKey()
        {
            var key = Environment.GetEnvironmentVariable(SessionEncodingKeyEnv);
            if (string.IsNullOrEmpty(key))
                throw new Exception($"Env Variable {SessionEncodingKeyEnv} is not found");
            
            return key;
        }
    }
}