using recibos.features.Receipts.Presentation.ViewModels;

namespace recibos.features.Receipts.Presentation.Pages {
    public partial class ReceiptDetailPage : ContentPage {
        private readonly ReceiptDetailViewModel _viewModel;

        public ReceiptDetailPage(ReceiptDetailViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private void ClearSignature_Clicked(object sender, EventArgs e) {
            SignatureDrawingView.Clear();
        }

        private async void SaveButton_Clicked(object sender, EventArgs e) {
            // Guardar la firma antes de guardar el recibo
            if (SignatureDrawingView.Lines.Count > 0) {
                await _viewModel.SaveSignatureFromDrawingView(SignatureDrawingView);
            }

            // Continuar con el guardado normal
            await _viewModel.Save();
        }
    }
}