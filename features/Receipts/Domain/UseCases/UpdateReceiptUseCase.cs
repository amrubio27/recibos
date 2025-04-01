using FluentResults;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Domain.UseCases {
    public class UpdateReceiptUseCase {
        private readonly IReceiptRepository _receiptRepository;

        public UpdateReceiptUseCase(IReceiptRepository receiptRepository) {
            _receiptRepository = receiptRepository;
        }

        public async Task<Result<Receipt>> ExecuteAsync(Receipt updatedReceipt) {
            // Validate input
            if (updatedReceipt == null) {
                return Result.Fail("El recibo proporcionado para actualizar es nulo.");
            }

            if (updatedReceipt.Id <= 0) {
                return Result.Fail("El ID del recibo para actualizar no es válido.");
            }

            // Add business logic/validation specific to updating a receipt
            // Removed Provider validation
            // Removed Amount validation

            // Example validation for Matricula (assuming it's required)
            if (string.IsNullOrWhiteSpace(updatedReceipt.Matricula)) {
                return Result.Fail("La matrícula del recibo no puede estar vacía.");
            }
            // ... other VALID validations for existing fields ...

            // Podría añadir lógica para actualizar campos como 'UpdatedAt'
            // updatedReceipt.UpdatedAt = DateTime.UtcNow;

            // Call repository method and return its Result
            return await _receiptRepository.UpdateReceiptAsync(updatedReceipt);
        }
    }
}