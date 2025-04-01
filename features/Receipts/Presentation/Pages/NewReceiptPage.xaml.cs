using recibos.features.Receipts.Presentation.ViewModels;

namespace recibos.features.Receipts.Presentation.Pages {
    public partial class NewReceiptPage : ContentPage {
        private readonly NewReceiptViewModel _viewModel;

        public NewReceiptPage(NewReceiptViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}