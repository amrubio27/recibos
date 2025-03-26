using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Presentation.Models
{
    public class ReceiptPresentationMapper : IReceiptPresentationMapper
    {
        public ReceiptListItemModel DomainToPresentation(Receipt receipt)
        {
            return new ReceiptListItemModel
            {
                Id = receipt.Id,
                Title = receipt.Title,
                Matricula = receipt.Matricula,
                Nota = receipt.Nota,
                CreatedAt = receipt.CreatedAt
            };
        }
        
        public ReceiptDetailModel DomainToDetailPresentation(Receipt receipt)
        {
            return new ReceiptDetailModel
            {
                Id = receipt.Id,
                Title = receipt.Title,
                Matricula = receipt.Matricula,
                Nota = receipt.Nota,
                SignatureBase64 = receipt.SignatureBase64,
                NoSignatureRequired = receipt.NoSignatureRequired,
                IsDescarga = receipt.IsDescarga,
                CreatedAt = receipt.CreatedAt,
                Latitude = receipt.Latitude,
                Longitude = receipt.Longitude,
                LocationDescription = receipt.LocationDescription
            };
        }

        public Receipt PresentationToDomain(ReceiptListItemModel model)
        {
            return new Receipt
            {
                Id = model.Id,
                Title = model.Title,
                Matricula = model.Matricula,
                Nota = model.Nota,
                CreatedAt = model.CreatedAt
            };
        }
        
        public Receipt PresentationDetailToDomain(ReceiptDetailModel model)
        {
            return new Receipt
            {
                Id = model.Id,
                Title = model.Title,
                Matricula = model.Matricula,
                Nota = model.Nota,
                SignatureBase64 = model.SignatureBase64,
                NoSignatureRequired = model.NoSignatureRequired,
                IsDescarga = model.IsDescarga,
                CreatedAt = model.CreatedAt,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                LocationDescription = model.LocationDescription
            };
        }
    }
}