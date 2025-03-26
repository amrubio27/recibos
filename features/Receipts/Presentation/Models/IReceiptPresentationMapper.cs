using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Presentation.Models;

public interface IReceiptPresentationMapper {
    // Conversión de dominio a presentación
    ReceiptListItemModel DomainToPresentation(Receipt receipt);
    ReceiptDetailModel DomainToDetailPresentation(Receipt receipt);
        
    // Conversión de presentación a dominio
    Receipt PresentationToDomain(ReceiptListItemModel model);
    Receipt PresentationDetailToDomain(ReceiptDetailModel model);
}