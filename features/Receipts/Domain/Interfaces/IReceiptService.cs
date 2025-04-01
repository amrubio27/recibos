using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Domain.Interfaces {
    public interface IReceiptService {
        Task<List<Receipt>> GetReceiptsAsync();
        Task<Receipt> GetReceiptAsync(int id);
        Task<Receipt> AddReceiptAsync(Receipt receipt);
        Task<Receipt> UpdateReceiptAsync(Receipt receipt);
        Task<bool> DeleteReceiptAsync(int id);
    }
}