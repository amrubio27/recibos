namespace recibos.core.data.services.location;

public class LocationService : ILocationService
{
    public async Task<LocationResult> GetCurrentLocationAsync()
    {
        try
        {
            var status = await CheckAndRequestLocationPermission();
            if (status != PermissionStatus.Granted)
            {
                return new LocationResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "Permiso de ubicación denegado" 
                };
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location == null)
                return new LocationResult { IsSuccess = false, ErrorMessage = "No se pudo obtener la ubicación" };

            return new LocationResult
            {
                IsSuccess = true,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };
        }
        catch (FeatureNotSupportedException)
        {
            return new LocationResult 
            { 
                IsSuccess = false, 
                ErrorMessage = "Ubicación no soportada en este dispositivo" 
            };
        }
        catch (PermissionException)
        {
            return new LocationResult 
            { 
                IsSuccess = false, 
                ErrorMessage = "Permisos de ubicación requeridos" 
            };
        }
        catch (Exception ex)
        {
            return new LocationResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"Error: {ex.Message}" 
            };
        }
    }

    public async Task<string> GetLocationDescriptionAsync(double latitude, double longitude)
    {
        try
        {
            var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks?.FirstOrDefault();
            
            if (placemark != null)
            {
                return $"{placemark.Locality}, {placemark.AdminArea}, {placemark.CountryName}";
            }
            return "Ubicación desconocida";
        }
        catch
        {
            return "No se pudo obtener descripción de la ubicación";
        }
    }

    private static async Task<PermissionStatus> CheckAndRequestLocationPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status;
    }
}