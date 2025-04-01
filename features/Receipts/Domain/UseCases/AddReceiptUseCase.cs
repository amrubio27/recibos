using FluentResults;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Domain.UseCases {
    public class AddReceiptUseCase {
        private readonly IReceiptRepository _receiptRepository;

        public AddReceiptUseCase(IReceiptRepository receiptRepository) {
            _receiptRepository = receiptRepository;
        }

        public async Task<Result<Receipt>> ExecuteAsync(Receipt newReceipt) {
            // Validate input
            if (newReceipt == null) {
                // Return Fail instead of throwing exception
                return Result.Fail("El recibo proporcionado es nulo.");
            }

            // Add business logic/validation specific to adding a receipt
            // Example: Ensure required fields are present

            // ... other validations ...

            // Set default values or perform other logic
            newReceipt.CreatedAt = DateTime.UtcNow; // Ensure creation date

            // Call repository method and return its Result
            return await _receiptRepository.AddReceiptAsync(newReceipt);
        }
    }
}