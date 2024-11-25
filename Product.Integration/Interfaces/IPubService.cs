using Microsoft.Extensions.Logging;

namespace Product.Integration.Interfaces
{
    public interface IPubService
    {
        abstract static void Initialize(ILogger logger);
        abstract static void InitializePublisher();
        abstract static void PublishMessage(string topic, string message);
        abstract static void Dispose();
    }
}
