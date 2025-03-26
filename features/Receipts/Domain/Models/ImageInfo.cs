using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace recibos.features.Receipts.Domain.Models
{
    public class ImageInfo : INotifyPropertyChanged
    {
        private string _base64;
        private ImageSource _source;

        public string Base64
        {
            get => _base64;
            set
            {
                _base64 = value;
                OnPropertyChanged();
            }
        }

        public ImageSource Source
        {
            get => _source;
            set
            {
                _source = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
