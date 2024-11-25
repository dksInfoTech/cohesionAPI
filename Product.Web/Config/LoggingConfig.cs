using Amazon;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using Amazon.CloudWatchLogs;

namespace Product.Web.Config
{
    public static class LoggingConfig
    {
        public static void ConfigureLogging(IServiceCollection services, IConfiguration configuration)
        {
            var configure = configuration;

            var selectedProvider = configuration["Logging:SelectedProvider"];
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(configuration["Logging:LogLevel:Default"]))
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext();

            if (selectedProvider == "File")
            {
                var filePath = configuration["Logging:File:Path"];
                var fileLogLevel = Enum.Parse<Serilog.Events.LogEventLevel>(configuration["Logging:File:LogLevel"]);

                loggerConfiguration = loggerConfiguration
                    .WriteTo.File(filePath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: fileLogLevel);
            }
            else if (selectedProvider == "CloudWatch")
            {
                var cloudWatchOptions = new CloudWatchSinkOptions
                {
                    LogGroupName = configuration["Logging:CloudWatch:LogGroup"],
                    MinimumLogEventLevel = Enum.Parse<Serilog.Events.LogEventLevel>(configuration["Logging:CloudWatch:LogLevel"]),
                    CreateLogGroup = true,
                };

                var cloudWatchClient = new AmazonCloudWatchLogsClient(RegionEndpoint.GetBySystemName(configuration["Logging:CloudWatch:Region"]));

                loggerConfiguration = loggerConfiguration
                    .WriteTo.AmazonCloudWatch(cloudWatchOptions, cloudWatchClient);
            }
            else if (selectedProvider == "Splunk")
            {
                var splunkUrl = configuration["Logging:Splunk:Endpoint"];
                var splunkToken = configuration["Logging:Splunk:Token"];
                var splunkLogLevel = Enum.Parse<Serilog.Events.LogEventLevel>(configuration["Logging:Splunk:LogLevel"]);

                loggerConfiguration = loggerConfiguration
                    .WriteTo.EventCollector(splunkUrl, splunkToken, restrictedToMinimumLevel: splunkLogLevel);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            services.AddLogging(builder => builder.AddSerilog());
        }
    }
}
