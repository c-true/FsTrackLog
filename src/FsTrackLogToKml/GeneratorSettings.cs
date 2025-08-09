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

    public TourConfig TourConfig { get; set; }

    public KmlStyleConfig StyleConfig { get; set; }
}