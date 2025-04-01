using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using recibos.core.data.services.location;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Domain.Models;
using recibos.features.Receipts.Presentation.Models;

namespace recibos.features.Receipts.Presentation.ViewModels {
    public partial class NewReceiptViewModel : ObservableObject {
        private readonly ILocationService _locationService;
        private readonly IReceiptPresentationMapper _mapper;
        private readonly IReceiptService _receiptService;

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
            IReceiptService receiptService,
            IReceiptPresentationMapper mapper,
            ILocationService locationService) {
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            Photos = new ObservableCollection<ImageInfo>();
        }

        public bool IsNotBusy => !IsBusy;

        public string OperationType =>
            IsDescarga ? Resources.Strings.AppResources.UnloadLabel : Resources.Strings.AppResources.LoadLabel;

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
                    await Shell.Current.DisplayAlert("Permiso denegado",
                        "No se puede acceder a la cámara. Revisa los permisos de la aplicación.", "OK");
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
            if (drawingView == null || drawingView.Lines.Count <= 0) return;

            try {
                using var stream = await drawingView.GetImageStream(300, 100);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] bytes = memoryStream.ToArray();

                SignatureBase64 = Convert.ToBase64String(bytes);
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al guardar firma: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la firma", "OK");
            }
        }

        public async Task SaveReceipt() {
            if (string.IsNullOrWhiteSpace(Matricula)) {
                await Shell.Current.DisplayAlert("Datos incompletos", "La matrícula es obligatoria", "OK");
                return;
            }

            IsBusy = true;

            try {
                // Crear modelo de presentación
                var receiptModel = new ReceiptDetailModel {
                    Title = Title ?? string.Empty,
                    Matricula = Matricula,
                    Nota = Nota,
                    SignatureBase64 = NoSignatureRequired ? null : SignatureBase64,
                    NoSignatureRequired = NoSignatureRequired,
                    IsDescarga = IsDescarga,
                    CreatedAt = DateTime.Now,
                    Latitude = Latitude,
                    Longitude = Longitude,
                    LocationDescription = LocationDescription
                };

                // Convertir a modelo de dominio
                var domainReceipt = _mapper.PresentationDetailToDomain(receiptModel);

                // Guardar las fotos en el modelo de dominio
                domainReceipt.PhotosBase64 = Photos.Select(p => p.Base64).ToList();

                // Guardar mediante el servicio de dominio
                var result = await _receiptService.AddReceiptAsync(domainReceipt);

                if (result != null) {
                    await Shell.Current.DisplayAlert("Éxito", "Recibo guardado correctamente", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else {
                    await Shell.Current.DisplayAlert("Error", "No se pudo guardar el recibo", "OK");
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al guardar recibo: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar el recibo", "OK");
            }
            finally {
                IsBusy = false;
            }
        }

        private static async Task<PermissionStatus> CheckAndRequestCameraPermission() {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted) {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            return status;
        }
    }
}