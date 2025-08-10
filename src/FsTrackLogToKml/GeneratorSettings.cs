namespace FsTrackLogToKml;

public class GeneratorSettings
{
    /// <summary>
    /// The base directory for tour files. Must not be changed.
    /// </summary>
    public string BaseDir { get; set; }

    /// <summary>
    /// Work directory for generating KML file(s)
    /// </summary>
    public string WorkFolder { get; set; }

    /// <summary>
    /// The current configuration for the tour.
    /// </summary>
    public TourConfig TourConfig { get; set; }

    /// <summary>
    /// The current configuration for styles for plans, routes and tracks.
    /// </summary>
    public KmlStyleConfig StyleConfig { get; set; }
}