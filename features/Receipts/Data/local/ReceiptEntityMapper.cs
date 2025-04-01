using System.Diagnostics;
using Newtonsoft.Json;
using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Data.local;

public static class ReceiptEntityMapper {
    public static Receipt ToModel(ReceiptEntity entity) {
        if (entity == null) return null;

        var model = new Receipt {
            Id = entity.Id,
            Title = entity.Title,
            Matricula = entity.Matricula,
            Nota = entity.Nota,
            SignatureBase64 = entity.SignatureBase64,
            PhotosBase64 = DeserializePhotos(entity.PhotosBase64Json),
            NoSignatureRequired = entity.NoSignatureRequired,
            IsDescarga = entity.IsDescarga,
            CreatedAt = entity.CreatedAt,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            LocationDescription = entity.LocationDescription
        };

        return model;
    }

    public static ReceiptEntity ToEntity(Receipt model) {
        if (model == null) return null;

        var entity = new ReceiptEntity {
            Id = model.Id,
            Title = model.Title ?? string.Empty,
            Matricula = model.Matricula,
            Nota = model.Nota ?? string.Empty,
            SignatureBase64 = model.SignatureBase64,
            PhotosBase64Json = SerializePhotos(model.PhotosBase64),
            NoSignatureRequired = model.NoSignatureRequired,
            IsDescarga = model.IsDescarga,
            CreatedAt = model.CreatedAt,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            LocationDescription = model.LocationDescription
        };

        return entity;
    }

    private static List<string> DeserializePhotos(string json) {
        try {
            if (string.IsNullOrEmpty(json))
                return new List<string>();

            var list = JsonConvert.DeserializeObject<List<string>>(json);
            Debug.WriteLine($"Deserializado correctamente: {list?.Count ?? 0} fotos");
            return list ?? new List<string>();
        }
        catch (Exception ex) {
            Debug.WriteLine($"Error al deserializar JSON de fotos: {ex.Message}. JSON: {json}");
            return new List<string>();
        }
    }

    private static string SerializePhotos(List<string> photos) {
        try {
            if (photos == null || photos.Count == 0) {
                return "[]";
            }

            var json = JsonConvert.SerializeObject(photos);
            Debug.WriteLine($"Serializado correctamente: {photos.Count} fotos en JSON");
            return json;
        }
        catch (Exception ex) {
            Debug.WriteLine($"Error al serializar lista de fotos: {ex.Message}");
            return "[]";
        }
    }
}