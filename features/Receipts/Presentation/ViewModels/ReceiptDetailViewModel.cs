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
    [QueryProperty(nameof(ReceiptId), "id")]
    public partial class ReceiptDetailViewModel : ObservableObject {
        private readonly IReceiptService _receiptService;
        private readonly IReceiptPresentationMapper _mapper;
        private readonly ILocationService _locationService;

        [ObservableProperty] private ReceiptDetailModel _receipt;
        [ObservableProperty] private string _receiptId;
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;
        public bool IsNotLoading => !IsLoading;
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotEditing))]
        private bool _isEditing;
        public bool IsNotEditing => !IsEditing;
        [ObservableProperty] private ImageSource _signatureImage;
        [ObservableProperty] private ObservableCollection<ImageInfo> _photos;
        [ObservableProperty] private bool _isLocationEnabled;
        [ObservableProperty] private bool _isCapturingLocation;
        

        public ReceiptDetailViewModel(
            IReceiptService receiptService, 
            IReceiptPresentationMapper mapper,
            ILocationService locationService) {
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            Photos = new ObservableCollection<ImageInfo>();
        }

        partial void OnReceiptIdChanged(string value) {
            try {
                if (int.TryParse(value, out int id)) {
                    LoadReceipt(id);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al procesar ID: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task Save() {
            if (Receipt == null) return;
            
            if (string.IsNullOrWhiteSpace(Receipt.Matricula)) {
                await Shell.Current.DisplayAlert("Datos incompletos", "La matrícula es obligatoria", "OK");
                return;
            }

            try {
                Receipt.Title = Receipt.Title ?? string.Empty;
                Receipt.Nota = Receipt.Nota ?? string.Empty;
                var photosBase64 = Photos.Select(p => p.Base64).ToList();

                // Transferir datos al dominio mediante el mapper
                var domainReceipt = _mapper.PresentationDetailToDomain(Receipt);
                domainReceipt.PhotosBase64 = photosBase64;
                domainReceipt.SignatureBase64 = Receipt.SignatureBase64;

                // Determinar si es una creación o actualización
                bool esNuevo = Receipt.Id == 0;
                var updatedReceipt = esNuevo 
                    ? await _receiptService.AddReceiptAsync(domainReceipt)
                    : await _receiptService.UpdateReceiptAsync(domainReceipt);

                // Actualizar el modelo de presentación con el resultado
                if (updatedReceipt != null) {
                    Receipt = _mapper.DomainToDetailPresentation(updatedReceipt);
                    LoadPhotosAndSignature();
                    await Shell.Current.DisplayAlert("Éxito", 
                        esNuevo ? "Recibo creado correctamente" : "Recibo actualizado correctamente", "OK");
            
                    // Si es un recibo nuevo, navegar hacia atrás
                    if (esNuevo) {
                        await Shell.Current.GoToAsync("..");
                        return;
                    }
                }
                else {
                    await Shell.Current.DisplayAlert("Error", 
                        esNuevo ? "No se pudo crear el recibo" : "No se pudo actualizar el recibo", "OK");
                }

                IsEditing = false;
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al guardar recibo: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar el recibo", "OK");
            }
        }

        [RelayCommand]
        private async Task Delete() {
            if (Receipt == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmar eliminación",
                "¿Estás seguro de que deseas eliminar este recibo?",
                "Sí", "No");

            if (!confirm) return;

            try {
                bool success = await _receiptService.DeleteReceiptAsync(Receipt.Id);

                if (success) {
                    await Shell.Current.GoToAsync("..");
                }
                else {
                    await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el recibo", "OK");
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al eliminar recibo: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el recibo", "OK");
            }
        }

        [RelayCommand]
        private void Edit() => IsEditing = true;

        [RelayCommand]
        private async Task Cancel() 
        {
            if (Receipt?.Id == 0) 
            {
                // Es un recibo nuevo, volver a la pantalla anterior
                await Shell.Current.GoToAsync("..");
            }
            else 
            {
                // Es un recibo existente, salir del modo edición
                IsEditing = false;
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
                    await Shell.Current.DisplayAlert(
                        "Permiso denegado",
                        "No se puede acceder a la cámara. Revisa los permisos de la aplicación.",
                        "OK");
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
        private async Task CaptureLocation()
        {
            if (IsCapturingLocation) return;
        
            IsCapturingLocation = true;
        
            try
            {
                var result = await _locationService.GetCurrentLocationAsync();
            
                if (result.IsSuccess)
                {
                    Receipt.Latitude = result.Latitude;
                    Receipt.Longitude = result.Longitude;
                    Receipt.LocationDescription = await _locationService.GetLocationDescriptionAsync(
                        result.Latitude.Value, result.Longitude.Value);
                    IsLocationEnabled = true;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", result.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al capturar ubicación: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo capturar la ubicación", "OK");
            }
            finally
            {
                IsCapturingLocation = false;
            }
        }
        
        [RelayCommand]
        private void ClearLocation()
        {
            Receipt.Latitude = null;
            Receipt.Longitude = null;
            Receipt.LocationDescription = null;
            IsLocationEnabled = false;
        }

        public async Task SaveSignatureFromDrawingView(IDrawingView drawingView) {
            // Si no requiere firma, no hacer nada
            if (Receipt?.NoSignatureRequired == true) return;
            if (drawingView == null || drawingView.Lines.Count <= 0) return;

            try {
                using var stream = await drawingView.GetImageStream(imageSizeWidth: 300, imageSizeHeight: 100);
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

            try {
                var domainReceipt = await _receiptService.GetReceiptAsync(receiptId);

                if (domainReceipt == null) {
                    Debug.WriteLine($"Recibo no encontrado con ID: {receiptId}");
                    await Shell.Current.DisplayAlert("Error", "Recibo no encontrado", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Convertir a modelo de presentación
                Receipt = _mapper.DomainToDetailPresentation(domainReceipt);
                
                IsLocationEnabled = Receipt.Latitude.HasValue && Receipt.Longitude.HasValue;

                LoadPhotosAndSignature();
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al cargar recibo: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo cargar el recibo", "OK");
            }
            finally {
                IsLoading = false;
            }
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
                // Carga lazy y caché de imágenes
                var domainReceipt = await _receiptService.GetReceiptAsync(Receipt.Id);

                if (domainReceipt?.PhotosBase64?.Any() != true)
                    return;

                // Procesar en paralelo para mejorar rendimiento
                var tasks = domainReceipt.PhotosBase64
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
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted) {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            return status;
        }
    }
}