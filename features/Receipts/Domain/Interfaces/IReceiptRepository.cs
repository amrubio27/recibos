using FluentResults;
using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Domain.Interfaces {
    public interface IReceiptRepository {
        Task<Result<List<Receipt>>> GetReceiptsAsync();
        Task<Result<Receipt>> GetReceiptAsync(int id);
        Task<Result<Receipt>> AddReceiptAsync(Receipt receipt);
        Task<Result<Receipt>> UpdateReceiptAsync(Receipt receipt);
        Task<Result> DeleteReceiptAsync(int id);
    }
}