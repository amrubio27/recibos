using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Presentation.Models;
using recibos.features.Receipts.Presentation.Pages;

namespace recibos.features.Receipts.Presentation.ViewModels {
    public partial class ReceiptsViewModel : ObservableObject {
        private readonly IReceiptService _receiptService;
        private readonly IReceiptPresentationMapper _mapper;

        [ObservableProperty] private ObservableCollection<ReceiptListItemModel> _receipts;
        [ObservableProperty] private ObservableCollection<ReceiptListItemModel> _filteredReceipts;
        [ObservableProperty] private string _searchText;
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;
        [ObservableProperty] private bool _isRefreshing;

        partial void OnSearchTextChanged(string value) {
            ApplyFilter();
        }
        
        private void ApplyFilter() {
            try {
                FilteredReceipts.Clear();

                if (string.IsNullOrWhiteSpace(SearchText)) {
                    foreach (var receipt in Receipts)
                        FilteredReceipts.Add(receipt);
                    return;
                }

                string searchLower = SearchText.ToLowerInvariant();

                foreach (var receipt in Receipts) {
                    bool titleMatch = !string.IsNullOrEmpty(receipt.Title) &&
                                      receipt.Title.ToLowerInvariant().Contains(searchLower);
                    bool matriculaMatch = !string.IsNullOrEmpty(receipt.Matricula) &&
                                          receipt.Matricula.ToLowerInvariant().Contains(searchLower);

                    if (titleMatch || matriculaMatch) {
                        FilteredReceipts.Add(receipt);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al aplicar filtro: {ex.Message}");
                // Asegurar que se muestre algo
                FilteredReceipts.Clear();
                foreach (var receipt in Receipts)
                    FilteredReceipts.Add(receipt);
            }
        }

        public ReceiptsViewModel(IReceiptService receiptService, IReceiptPresentationMapper mapper) {
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Receipts = new ObservableCollection<ReceiptListItemModel>();
            FilteredReceipts = new ObservableCollection<ReceiptListItemModel>();
        }

        [RelayCommand]
        public async Task LoadReceipts() {
            if (IsBusy)
                return;

            IsBusy = true;

            try {
                Receipts.Clear();
                var domainReceipts = await _receiptService.GetReceiptsAsync();

                // Ordenar los recibos por fecha, del más reciente al más antiguo
                var orderedReceipts = domainReceipts.OrderByDescending(r => r.CreatedAt).ToList();

                foreach (var receipt in orderedReceipts) {
                    var presentationModel = _mapper.DomainToPresentation(receipt);
                    Receipts.Add(presentationModel);
                }

                ApplyFilter();
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al cargar recibos: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudieron cargar los recibos", "Aceptar");
            }
            finally {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToReceiptDetails(ReceiptListItemModel receipt) {
            if (receipt == null)
                return;

            var parameters = new Dictionary<string, object> {
                { "id", receipt.Id.ToString() }
            };

            await Shell.Current.GoToAsync(nameof(ReceiptDetailPage), parameters);
        }

        [RelayCommand]
        private async Task AddReceipt() {
            await Shell.Current.GoToAsync(nameof(NewReceiptPage));
        }

        [RelayCommand]
        private async Task Refresh() {
            if (IsBusy)
                return;

            IsRefreshing = true;
            IsBusy = true;

            try {
                // Your refresh logic here
                Receipts.Clear();
                var domainReceipts = await _receiptService.GetReceiptsAsync();
                var orderedReceipts = domainReceipts.OrderByDescending(r => r.CreatedAt).ToList();

                foreach (var receipt in orderedReceipts) {
                    var presentationModel = _mapper.DomainToPresentation(receipt);
                    Receipts.Add(presentationModel);
                }

                ApplyFilter();
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al refrescar recibos: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudieron refrescar los recibos", "Aceptar");
            }
            finally {
                IsBusy = false;
                IsRefreshing = false;
            }
        }
    }
}