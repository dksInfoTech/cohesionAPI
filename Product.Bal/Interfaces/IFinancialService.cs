using Product.Bal.Models;

namespace Product.Bal.Interfaces
{
    public interface IFinancialService
    {
        Task<bool> UpdateFinancialExtractJobStatus(FdeStatusUpdate response);
    }
}
