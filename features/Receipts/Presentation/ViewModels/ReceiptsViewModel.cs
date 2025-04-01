using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using recibos.features.Receipts.Domain.Models;
using recibos.features.Receipts.Domain.UseCases;
using recibos.features.Receipts.Presentation.Models;
using recibos.features.Receipts.Presentation.Pages;

namespace recibos.features.Receipts.Presentation.ViewModels {
    public partial class ReceiptsViewModel : ObservableObject {
        private readonly GetReceiptsUseCase _getReceiptsUseCase;
        private readonly IReceiptPresentationMapper _mapper;

        [ObservableProperty] private string _errorMessage;

        [ObservableProperty] private ObservableCollection<ReceiptListItemModel> _filteredReceipts;

        [ObservableProperty] private bool _hasError;

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty] private bool _isRefreshing;

        [ObservableProperty] private ObservableCollection<ReceiptListItemModel> _receipts;
        [ObservableProperty] private string _searchText;

        public ReceiptsViewModel(GetReceiptsUseCase getReceiptsUseCase, IReceiptPresentationMapper mapper) {
            _getReceiptsUseCase = getReceiptsUseCase ?? throw new ArgumentNullException(nameof(getReceiptsUseCase));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Receipts = new ObservableCollection<ReceiptListItemModel>();
            FilteredReceipts = new ObservableCollection<ReceiptListItemModel>();
        }

        public bool IsNotBusy => !IsBusy;

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
                var foundItems = Receipts.Where(receipt =>
                    (!string.IsNullOrEmpty(receipt.Title) && receipt.Title.ToLowerInvariant().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(receipt.Matricula) &&
                     receipt.Matricula.ToLowerInvariant().Contains(searchLower))
                ).ToList();

                foreach (var receipt in foundItems) {
                    FilteredReceipts.Add(receipt);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al aplicar filtro: {ex.Message}");
                FilteredReceipts.Clear();
                foreach (var receipt in Receipts)
                    FilteredReceipts.Add(receipt);
            }
        }

        [RelayCommand]
        public async Task LoadReceipts() {
            if (IsBusy)
                return;

            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try {
                Receipts.Clear();
                Result<List<Receipt>> result = await _getReceiptsUseCase.ExecuteAsync();

                if (result.IsSuccess) {
                    var domainReceipts = result.Value;
                    var orderedReceipts = domainReceipts.OrderByDescending(r => r.CreatedAt).ToList();

                    foreach (var receipt in orderedReceipts) {
                        var presentationModel = _mapper.DomainToPresentation(receipt);
                        Receipts.Add(presentationModel);
                    }
                }
                else {
                    ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Error desconocido al cargar recibos.";
                    HasError = true;
                    Debug.WriteLine($"Error al cargar recibos: {ErrorMessage}");
                }

                ApplyFilter();
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error inesperado en LoadReceipts: {ex.Message}");
                ErrorMessage = $"Error inesperado: {ex.Message}";
                HasError = true;
                Receipts.Clear();
                FilteredReceipts.Clear();
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
            HasError = false;
            ErrorMessage = string.Empty;

            try {
                Receipts.Clear();
                Result<List<Receipt>> result = await _getReceiptsUseCase.ExecuteAsync();

                if (result.IsSuccess) {
                    var domainReceipts = result.Value;
                    var orderedReceipts = domainReceipts.OrderByDescending(r => r.CreatedAt).ToList();

                    foreach (var receipt in orderedReceipts) {
                        var presentationModel = _mapper.DomainToPresentation(receipt);
                        Receipts.Add(presentationModel);
                    }
                }
                else {
                    ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Error desconocido al refrescar recibos.";
                    HasError = true;
                    Debug.WriteLine($"Error al refrescar recibos: {ErrorMessage}");
                }

                ApplyFilter();
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error inesperado en Refresh: {ex.Message}");
                ErrorMessage = $"Error inesperado al refrescar: {ex.Message}";
                HasError = true;
                Receipts.Clear();
                FilteredReceipts.Clear();
            }
            finally {
                IsBusy = false;
                IsRefreshing = false;
            }
        }
    }
}