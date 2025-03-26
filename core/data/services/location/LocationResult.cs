namespace recibos.core.data.services.location;

public class LocationResult
{
    public bool IsSuccess { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string ErrorMessage { get; set; }
}