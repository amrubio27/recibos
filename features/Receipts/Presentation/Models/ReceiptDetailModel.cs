using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace recibos.features.Receipts.Presentation.Models;

public class ReceiptDetailModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Matricula { get; set; }
    public string Nota { get; set; }
    public string SignatureBase64 { get; set; }
    public bool NoSignatureRequired { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string LocationDescription { get; set; }
    public bool HasLocation => Latitude.HasValue && Longitude.HasValue;
    public DateTime CreatedAt { get; set; }
    public string FormattedDate => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    
    private bool _isDescarga;
    public bool IsDescarga
    {
        get => _isDescarga;
        set
        {
            if (_isDescarga != value)
            {
                _isDescarga = value;
                OnPropertyChanged();
            }
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}