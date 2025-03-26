namespace recibos.features.Receipts.Presentation.Models
{
    public class ReceiptListItemModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Matricula { get; set; }
        public string Nota { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string FormattedDate => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    }
}