using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Product.Integration;
using Product.Integration.Interfaces;
using System;

public class ZeroMqPubService: IPubService
{
    private static PublisherSocket _pubSocket;
    private static readonly object _lock = new object();
    private static bool _isInitialized = false; 
    private static ILogger _logger;

    public static ZeroMqPubService Instance => default;

    public static void Initialize(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static void InitializePublisher()
    {
        if (!_isInitialized)
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    _pubSocket = new PublisherSocket();
                    _pubSocket.Options.SendHighWatermark = 1000;
                    _pubSocket.Bind("tcp://*:5555");
                    _isInitialized = true;

                    Thread.Sleep(5000);

                    _logger.LogInformation("Publisher initialized and socket bound.");
                }
            }
        }
    }

    public static void PublishMessage(string topic, string message)
    {
        if (!_isInitialized) InitializePublisher();  // Ensure the publisher is initialized

        _logger.LogInformation($"Publishing: {topic} {message}");

        try
        {
            _pubSocket.SendMoreFrame(topic).SendFrame(message);
            _logger.LogInformation($"Published: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error Publishing: {ex.Message}");
        }
    }

    public static void Dispose()
    {
        _pubSocket?.Dispose();
        _logger.LogInformation("Publisher socket cleaned up.");
    }
}
