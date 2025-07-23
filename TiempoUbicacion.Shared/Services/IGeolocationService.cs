namespace TiempoUbicacion.Shared.Services
{
    public interface IGeolocationService
    {
        Task<(string Name, string FormattedLatitude, string FormattedLongitude, double Lat, double Lng)> GetCurrentLocationAsync();
    }
}
