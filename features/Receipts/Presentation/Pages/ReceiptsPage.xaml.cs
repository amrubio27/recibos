using recibos.features.Receipts.Presentation.ViewModels;

namespace recibos.features.Receipts.Presentation.Pages {
    public partial class ReceiptsPage : ContentPage {
        private readonly ReceiptsViewModel _viewModel;

        public ReceiptsPage(ReceiptsViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            _viewModel.LoadReceipts();
        }
    }
}