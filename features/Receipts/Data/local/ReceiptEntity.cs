using SQLite;

namespace recibos.features.Receipts.Data.local;

public class ReceiptEntity {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string Title { get; set; }
    public string Matricula { get; set; }
    public string Nota { get; set; }
    public string SignatureBase64 { get; set; }
    public string PhotosBase64Json { get; set; }
    public bool NoSignatureRequired { get; set; }
    public bool IsDescarga { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string LocationDescription { get; set; }
}