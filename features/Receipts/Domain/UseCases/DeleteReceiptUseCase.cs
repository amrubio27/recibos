using FluentResults;
using recibos.features.Receipts.Domain.Interfaces;

namespace recibos.features.Receipts.Domain.UseCases {
    public class DeleteReceiptUseCase {
        private readonly IReceiptRepository _receiptRepository;

        public DeleteReceiptUseCase(IReceiptRepository receiptRepository) {
            _receiptRepository = receiptRepository;
        }

        public async Task<Result> ExecuteAsync(int receiptId) {
            // Validate input
            if (receiptId <= 0) {
                // Return Fail instead of throwing exception
                return Result.Fail("El ID del recibo para eliminar no es válido.");
            }

            // Call repository method which now returns Result
            // and return its result directly.
            return await _receiptRepository.DeleteReceiptAsync(receiptId);
        }
    }
}