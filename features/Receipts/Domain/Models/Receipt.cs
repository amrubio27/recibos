using System.Diagnostics;
using Newtonsoft.Json;
using SQLite;

namespace recibos.features.Receipts.Domain.Models {
    public class Receipt {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Matricula { get; set; }
        public string Nota { get; set; }
        public string SignatureBase64 { get; set; }
        public List<string> PhotosBase64 { get; set; } = new List<string>();
        public bool NoSignatureRequired { get; set; }
        public bool IsDescarga { get; set; } = false; // false = Carga, true = Descarga
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string LocationDescription { get; set; }
    }
}