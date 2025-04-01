using System.Collections.ObjectModel;
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
    public partial class NewReceiptViewModel : ObservableObject {
        private readonly AddReceiptUseCase _addReceiptUseCase;
        private readonly ILocationService _locationService;
        private readonly IReceiptPresentationMapper _mapper;

        [ObservableProperty] private string _errorMessage;

        [ObservableProperty] private bool _hasError;

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty] private bool _isCapturingLocation;

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(OperationType))]
        private bool _isDescarga = false;

        [ObservableProperty] private bool _isLocationEnabled;
        [ObservableProperty] private double? _latitude;
        [ObservableProperty] private string _locationDescription;
        [ObservableProperty] private double? _longitude;
        [ObservableProperty] private string _matricula;
        [ObservableProperty] private bool _noSignatureRequired;
        [ObservableProperty] private string _nota;
        [ObservableProperty] private ObservableCollection<ImageInfo> _photos;
        [ObservableProperty] private string _signatureBase64;

        [ObservableProperty] private string _title;

        public NewReceiptViewModel(
            AddReceiptUseCase addReceiptUseCase,
            IReceiptPresentationMapper mapper,
            ILocationService locationService) {
            _addReceiptUseCase = addReceiptUseCase ?? throw new ArgumentNullException(nameof(addReceiptUseCase));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            Photos = new ObservableCollection<ImageInfo>();
        }

        public bool IsNotBusy => !IsBusy;

        public string OperationType =>
            IsDescarga ? AppResources.UnloadLabel : AppResources.LoadLabel;

        [RelayCommand]
        private async Task Cancel() => await Shell.Current.GoToAsync("..");

        [RelayCommand]
        private void ClearSignature() => SignatureBase64 = null;

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
                    Latitude = result.Latitude;
                    Longitude = result.Longitude;
                    LocationDescription = await _locationService.GetLocationDescriptionAsync(
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
            Latitude = null;
            Longitude = null;
            LocationDescription = null;
            IsLocationEnabled = false;
        }

        public async Task SaveSignatureFromDrawingView(IDrawingView drawingView) {
            if (NoSignatureRequired) return;
            if (drawingView == null) return;

            try {
                if (drawingView.Lines.Count > 0) {
                    using var stream = await drawingView.GetImageStream(300, 100);
                    if (stream != null) {
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        byte[] bytes = memoryStream.ToArray();
                        SignatureBase64 = Convert.ToBase64String(bytes);
                    }
                    else {
                        SignatureBase64 = null;
                    }
                }
                else {
                    SignatureBase64 = null;
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al guardar firma: {ex.Message}");
                SignatureBase64 = null;
            }
        }

        [RelayCommand]
        public async Task Save(object parameter) {
            if (parameter is IDrawingView drawingView) {
                await SaveSignatureFromDrawingView(drawingView);
            }
            else {
                Debug.WriteLine("Save command executed without a valid DrawingView parameter.");
            }

            if (string.IsNullOrWhiteSpace(Matricula)) {
                await Shell.Current.DisplayAlert("Datos incompletos", "La matrícula es obligatoria", "OK");
                return;
            }

            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try {
                var domainReceipt = new Receipt {
                    Id = 0,
                    Title = Title ?? string.Empty,
                    Matricula = Matricula,
                    Nota = Nota ?? string.Empty,
                    SignatureBase64 = NoSignatureRequired ? null : SignatureBase64,
                    NoSignatureRequired = NoSignatureRequired,
                    IsDescarga = IsDescarga,
                    CreatedAt = DateTime.UtcNow,
                    Latitude = Latitude,
                    Longitude = Longitude,
                    LocationDescription = LocationDescription,
                    PhotosBase64 = Photos.Select(p => p.Base64).ToList()
                };

                Result<Receipt> result = await _addReceiptUseCase.ExecuteAsync(domainReceipt);

                if (result.IsSuccess) {
                    Debug.WriteLine("Nuevo recibo guardado correctamente.");
                    await Shell.Current.DisplayAlert("Éxito", "Recibo guardado correctamente", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else {
                    ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Error desconocido al guardar el recibo.";
                    HasError = true;
                    Debug.WriteLine($"Error al guardar nuevo recibo: {ErrorMessage}");
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error inesperado al guardar recibo: {ex.Message}");
                ErrorMessage = $"Error inesperado: {ex.Message}";
                HasError = true;
            }
            finally {
                IsBusy = false;
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