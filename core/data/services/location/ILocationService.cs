namespace recibos.core.data.services.location;

public interface ILocationService
{
    Task<LocationResult> GetCurrentLocationAsync();
    Task<string> GetLocationDescriptionAsync(double latitude, double longitude);
}