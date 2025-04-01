using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using recibos.core.data.services.location;
using recibos.features.Receipts.Domain.Models;
using recibos.features.Receipts.Domain.UseCases;
using recibos.features.Receipts.Presentation.Models;
using recibos.Resources.Strings;

namespace recibos.features.Receipts.Presentation.ViewModels {
    [QueryProperty(nameof(ReceiptId), "id")]
    public partial class ReceiptDetailViewModel : ObservableObject {
        private readonly DeleteReceiptUseCase _deleteReceiptUseCase;
        private readonly GetReceiptDetailsUseCase _getReceiptDetailsUseCase;
        private readonly ILocationService _locationService;
        private readonly IReceiptPresentationMapper _mapper;
        private readonly UpdateReceiptUseCase _updateReceiptUseCase;

        [ObservableProperty] private string _errorMessage;

        [ObservableProperty] private bool _hasError;

        [ObservableProperty] private bool _isCapturingLocation;

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotEditing))]
        private bool _isEditing;

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        [ObservableProperty] private bool _isLocationEnabled;
        [ObservableProperty] private ObservableCollection<ImageInfo> _photos;

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(OperationType))]
        private ReceiptDetailModel _receipt;

        [ObservableProperty] private string _receiptId;
        [ObservableProperty] private ImageSource _signatureImage;

        public ReceiptDetailViewModel(
            GetReceiptDetailsUseCase getReceiptDetailsUseCase,
            UpdateReceiptUseCase updateReceiptUseCase,
            DeleteReceiptUseCase deleteReceiptUseCase,
            IReceiptPresentationMapper mapper,
            ILocationService locationService) {
            _getReceiptDetailsUseCase = getReceiptDetailsUseCase ??
                                        throw new ArgumentNullException(nameof(getReceiptDetailsUseCase));
            _updateReceiptUseCase =
                updateReceiptUseCase ?? throw new ArgumentNullException(nameof(updateReceiptUseCase));
            _deleteReceiptUseCase =
                deleteReceiptUseCase ?? throw new ArgumentNullException(nameof(deleteReceiptUseCase));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            Photos = new ObservableCollection<ImageInfo>();
        }

        public bool IsNotLoading => !IsLoading;
        public bool IsNotEditing => !IsEditing;

        public string OperationType => Receipt?.IsDescarga == true ? AppResources.UnloadLabel : AppResources.LoadLabel;

        partial void OnReceiptIdChanged(string value) {
            try {
                if (int.TryParse(value, out int id)) {
                    LoadReceipt(id);
                }
                else if (!string.IsNullOrEmpty(value)) {
                    IsLoading = false;
                    HasError = true;
                    ErrorMessage = $"El ID de recibo '{value}' no es válido.";
                    Debug.WriteLine($"Error: ID de recibo no válido: {value}");
                }
            }
            catch (Exception ex) {
                IsLoading = false;
                HasError = true;
                ErrorMessage = $"Error inesperado al procesar ID: {ex.Message}";
                Debug.WriteLine($"Error al procesar ID: {ex.Message}");
            }
        }

        partial void OnReceiptChanged(ReceiptDetailModel value) {
            if (value != null) {
                value.PropertyChanged -= Receipt_PropertyChanged;
                value.PropertyChanged += Receipt_PropertyChanged;
            }
        }

        private void Receipt_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(ReceiptDetailModel.IsDescarga)) {
                OnPropertyChanged(nameof(OperationType));
            }
        }

        [RelayCommand]
        public async Task Save() {
            if (Receipt == null) return;

            if (Receipt.Id <= 0) {
                Debug.WriteLine("Error: Intento de guardar un recibo sin ID válido desde ReceiptDetailViewModel.");
                await Shell.Current.DisplayAlert("Error", "No se puede guardar un recibo con ID inválido.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Receipt.Matricula)) {
                await Shell.Current.DisplayAlert("Datos incompletos", "La matrícula es obligatoria", "OK");
                return;
            }

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            Receipt.Title = Receipt.Title ?? string.Empty;
            Receipt.Nota = Receipt.Nota ?? string.Empty;
            var photosBase64 = Photos.Select(p => p.Base64).ToList();

            var domainReceipt = _mapper.PresentationDetailToDomain(Receipt);
            domainReceipt.PhotosBase64 = photosBase64;
            domainReceipt.SignatureBase64 = Receipt.SignatureBase64;

            Result<Receipt> result = await _updateReceiptUseCase.ExecuteAsync(domainReceipt);

            if (result.IsSuccess) {
                Receipt updatedDomainReceipt = result.Value;
                Receipt = _mapper.DomainToDetailPresentation(updatedDomainReceipt);
                LoadPhotosAndSignature();
                IsEditing = false;
                Debug.WriteLine($"Recibo {Receipt.Id} actualizado correctamente.");
                await Shell.Current.DisplayAlert("Éxito", "Recibo actualizado correctamente", "OK");
            }
            else {
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Error desconocido al guardar el recibo.";
                HasError = true;
                Debug.WriteLine($"Error al guardar recibo {Receipt.Id}: {ErrorMessage}");
            }

            IsLoading = false;
        }

        [RelayCommand]
        private async Task Delete() {
            if (Receipt == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmar eliminación",
                "¿Estás seguro de que deseas eliminar este recibo?",
                "Sí", "No");

            if (!confirm) return;

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            Result result = await _deleteReceiptUseCase.ExecuteAsync(Receipt.Id);

            if (result.IsSuccess) {
                Debug.WriteLine($"Recibo {Receipt.Id} eliminado correctamente.");
                await Shell.Current.GoToAsync("..");
            }
            else {
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Error desconocido al eliminar el recibo.";
                HasError = true;
                Debug.WriteLine($"Error al eliminar recibo {Receipt.Id}: {ErrorMessage}");
            }

            IsLoading = false;
        }

        [RelayCommand]
        private void Edit() => IsEditing = true;

        [RelayCommand]
        private async Task Cancel() {
            if (Receipt != null && Receipt.Id > 0) {
                IsEditing = false;
                LoadReceipt(Receipt.Id);
            }
            else {
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private void ClearSignature() {
            if (Receipt != null) {
                Receipt.SignatureBase64 = null;
                SignatureImage = null;
            }
        }

        [RelayCommand]
        private async Task TakePhoto() {
            try {
                if (!MediaPicker.Default.IsCaptureSupported) {
                    await Shell.Current.DisplayAlert("Error",
                        "La captura de fotos no está soportada en este dispositivo", "OK");
                    return;
                }

                var status = await CheckAndRequestCameraPermission();
                if (status != PermissionStatus.Granted) {
                    if (status != PermissionStatus.Denied || DeviceInfo.Platform != DevicePlatform.iOS) {
                        await Shell.Current.DisplayAlert("Permiso denegado", "No se puede acceder a la cámara.", "OK");
                    }

                    return;
                }

                var options = new MediaPickerOptions { Title = "Toma una foto" };
                var photo = await MediaPicker.Default.CapturePhotoAsync(options);

                if (photo != null) {
                    using var stream = await photo.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();

                    string base64 = Convert.ToBase64String(imageBytes);
                    var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                    Photos.Add(new ImageInfo { Base64 = base64, Source = imageSource });
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al tomar foto: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo tomar la foto", "OK");
            }
        }

        [RelayCommand]
        private void DeletePhoto(ImageInfo photo) {
            if (photo != null) {
                Photos.Remove(photo);
            }
        }

        [RelayCommand]
        private async Task CaptureLocation() {
            if (IsCapturingLocation) return;

            IsCapturingLocation = true;

            try {
                var result = await _locationService.GetCurrentLocationAsync();

                if (result.IsSuccess) {
                    Receipt.Latitude = result.Latitude;
                    Receipt.Longitude = result.Longitude;
                    Receipt.LocationDescription = await _locationService.GetLocationDescriptionAsync(
                        result.Latitude.Value, result.Longitude.Value);
                    IsLocationEnabled = true;
                }
                else {
                    await Shell.Current.DisplayAlert("Error", result.ErrorMessage, "OK");
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al capturar ubicación: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo capturar la ubicación", "OK");
            }
            finally {
                IsCapturingLocation = false;
            }
        }

        [RelayCommand]
        private void ClearLocation() {
            Receipt.Latitude = null;
            Receipt.Longitude = null;
            Receipt.LocationDescription = null;
            IsLocationEnabled = false;
        }

        public async Task SaveSignatureFromDrawingView(IDrawingView drawingView) {
            if (Receipt?.NoSignatureRequired == true) return;
            if (drawingView == null || drawingView.Lines.Count <= 0) return;

            try {
                using var stream = await drawingView.GetImageStream(
                    300,
                    100);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] bytes = memoryStream.ToArray();

                if (Receipt != null) {
                    Receipt.SignatureBase64 = Convert.ToBase64String(bytes);
                    SignatureImage = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al guardar firma: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la firma", "OK");
            }
        }

        private async void LoadReceipt(int receiptId) {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            Result<Receipt> result = await _getReceiptDetailsUseCase.ExecuteAsync(receiptId);

            if (result.IsSuccess) {
                Receipt domainReceipt = result.Value;
                Receipt = _mapper.DomainToDetailPresentation(domainReceipt);
                IsLocationEnabled = Receipt.Latitude.HasValue && Receipt.Longitude.HasValue;
                LoadPhotosAndSignature();
            }
            else {
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Error desconocido al cargar el recibo.";
                HasError = true;
                Debug.WriteLine($"Error al cargar recibo {receiptId}: {ErrorMessage}");
            }

            IsLoading = false;
        }

        private void LoadPhotosAndSignature() {
            LoadSignatureImage();
            LoadPhotos();
        }

        private void LoadSignatureImage() {
            if (string.IsNullOrEmpty(Receipt?.SignatureBase64)) return;

            try {
                byte[] bytes = Convert.FromBase64String(Receipt.SignatureBase64);
                SignatureImage = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al cargar imagen de firma: {ex.Message}");
            }
        }

        private async Task LoadPhotos() {
            Photos.Clear();

            if (Receipt?.Id == 0) return;

            try {
                var domainReceipt = await _getReceiptDetailsUseCase.ExecuteAsync(Receipt.Id);

                if (domainReceipt?.Value.PhotosBase64?.Any() != true)
                    return;

                var tasks = domainReceipt.Value.PhotosBase64
                    .Where(base64 => !string.IsNullOrEmpty(base64))
                    .Select(async base64 => {
                        try {
                            byte[] bytes = Convert.FromBase64String(base64);
                            var imageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
                            return new ImageInfo { Base64 = base64, Source = imageSource };
                        }
                        catch (Exception ex) {
                            Debug.WriteLine($"Error procesando foto: {ex.Message}");
                            return null;
                        }
                    });

                var results = await Task.WhenAll(tasks);

                foreach (var photo in results.Where(p => p != null)) {
                    Photos.Add(photo);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error general cargando fotos: {ex.Message}");
            }
        }

        private static async Task<PermissionStatus> CheckAndRequestCameraPermission() {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS) {
                await Shell.Current.DisplayAlert("Permiso requerido",
                    "El permiso de cámara fue denegado. Por favor, habilítalo en los ajustes.", "OK");
                return status;
            }

            if (Permissions.ShouldShowRationale<Permissions.Camera>()) {
                await Shell.Current.DisplayAlert("Permiso requerido",
                    "Se necesita acceso a la cámara para tomar fotos del recibo.", "OK");
            }

            status = await Permissions.RequestAsync<Permissions.Camera>();

            return status;
        }
    }
}