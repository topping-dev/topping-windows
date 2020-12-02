namespace NdefLibrary
{
    /// <summary>
    /// Custom GeoCoordinate class to allow for cross-platform portability.
    /// </summary>
    /// <remarks>Windows Phone and Windows 8 use different namespaces &
    /// class name for the GeoCoordinate class.
    /// WP: System.Device.Location.GeoCoordinate,
    /// Win8: Windows.Devices.Geolocation.Geocoordinate</remarks>
    public class GeoCoordinate
    {
        /// <summary>
        /// The latitude in degrees.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude in degrees.
        /// </summary>
        public double Longitude { get; set; }
    }
}
