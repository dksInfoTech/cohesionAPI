namespace Product.Integration.Interfaces
{
    public interface IIntegrationAuthService
    {
        Task<string> GetToken();
    }
}
