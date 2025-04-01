using FluentResults;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Domain.UseCases {
    public class GetReceiptsUseCase {
        private readonly IReceiptRepository _receiptRepository;

        public GetReceiptsUseCase(IReceiptRepository receiptRepository) {
            _receiptRepository = receiptRepository;
        }

        public async Task<Result<List<Receipt>>> ExecuteAsync() {
            // Podría incluir lógica adicional aquí si fuera necesario (ordenación específica, filtrado inicial, etc.)
            return await _receiptRepository.GetReceiptsAsync();
        }
    }
}