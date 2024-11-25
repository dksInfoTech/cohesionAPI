using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using Product.Bal.Interfaces;
using Product.Bal.Models;
using Product.Integration.Constants;
using Product.Integration.Models;
using Product.Integration.Models.Data.Response;
using System.Text.Json;

namespace Product.Integration
{
    public class ZeroMqSubService : BackgroundService
    {
        private SubscriberSocket _subscriber;
        private readonly IntegrationConfig _integrationConfig;
        private readonly ILogger<ZeroMqSubService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ZeroMqSubService(IOptions<IntegrationConfig> integrationConfig, ILogger<ZeroMqSubService> logger, IServiceProvider serviceProvider)
        {
            _integrationConfig = integrationConfig.Value;
            _subscriber = new SubscriberSocket();
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var subscribeUrl = $"tcp://{_integrationConfig.ZeroMqConfig.ApiServiceSubUrl}:{_integrationConfig.ZeroMqConfig.ApiServiceSubPort}";
                _subscriber.Connect(subscribeUrl);
                _subscriber.Subscribe("");

                _logger.LogInformation("Started ZeroMQ subscriber at {Url}", subscribeUrl);

                var poller = new NetMQPoller { _subscriber };
                _subscriber.ReceiveReady += async (sender, args) =>
                {
                    var messageReceived = args.Socket.ReceiveFrameString();
                    await ProcessMessage(messageReceived);
                };

                poller.RunAsync();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000);  // Keep the task alive
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect or subscribe to ZeroMQ socket.");
            }
            finally
            {
                _subscriber?.Dispose();
                _logger.LogInformation("ZeroMQ subscriber disposed.");
            }
        }

        private async Task ProcessMessage(string messageReceived)
        {
            if (!string.IsNullOrEmpty(messageReceived))
            {
                int spaceIndex = messageReceived.IndexOf(' ');
                if (spaceIndex != -1)
                {
                    string topic = messageReceived.Substring(0, spaceIndex);
                    string message = messageReceived.Substring(spaceIndex + 1);
                    _logger.LogInformation("Received: Topic={Topic}, Message={Message}", topic, message);

                    switch (topic)
                    {
                        case IntegrationConstants.MqTopicStatusUpdate:
                            var data = JsonSerializer.Deserialize<FdeStatusUpdate>(message, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true  // Handle case insensitivity in JSON keys
                            });

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var financialService = scope.ServiceProvider.GetRequiredService<IFinancialService>();
                                await financialService.UpdateFinancialExtractJobStatus(data);
                            }

                            break;
                        default:
                            // Handle other topics or default case
                            break;
                    }
                }
                else
                {
                    _logger.LogError("Received a message without a space separator. Message: '{Message}'", messageReceived);
                }
            }
        }

        public override void Dispose()
        {
            _subscriber?.Dispose();
            base.Dispose();
            _logger.LogInformation("ZeroMq Subscriber Disposed.");
        }
    }
}
