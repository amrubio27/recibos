using FluentResults;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Domain.UseCases {
    public class GetReceiptDetailsUseCase {
        private readonly IReceiptRepository _receiptRepository;

        public GetReceiptDetailsUseCase(IReceiptRepository receiptRepository) {
            _receiptRepository = receiptRepository;
        }

        public async Task<Result<Receipt>> ExecuteAsync(int receiptId) {
            if (receiptId <= 0) {
                return Result.Fail("El ID del recibo no es válido.");
            }

            return await _receiptRepository.GetReceiptAsync(receiptId);
        }
    }
}